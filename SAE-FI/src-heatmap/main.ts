import * as THREE from "three";
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';

// --- Types ---
export interface SensorData {
    SensorId: string;
    Temperature: number;
}

export interface SensorPos {
    SensorId: string;
    Posiiton: THREE.Vector3;
}

declare global {
    interface Window {
        chrome?: {
            webview?: {
                addEventListener(type: string, listener: (event: { data: any }) => void): void;
            }
        }
    }
}

// --- Globals ---
let sensor_data: SensorData[] = [
    {
        SensorId: "S1",
        Temperature: 90 
    },
    {
        SensorId: "S2",
        Temperature: 90 
    },
    {
        SensorId: "S3",
        Temperature: 40 
    },
    {
        SensorId: "S4",
        Temperature: 40
    },
    {
        SensorId: "SB",
        Temperature: 30 
    },
    {
        SensorId: "SD",
        Temperature: 20 
    },
];

const VOXEL_DENSITY = 0.75;
const COLD = 20;
const WARM = 70;
const sensor_names: string[] = ["S1", "S2", "S3", "S4", "SB", "SD"];
let sensor_positions: SensorPos[] = [];

// --- WebView2 Listener ---
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', (event) => {
        console.log("Data Received:", event.data);
        sensor_data = event.data as SensorData[];
    });
} else {
    console.error("Page was not loaded in WebView2 environment.");
}

// --- Three.js Setup ---
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
const loader = new GLTFLoader();
const renderer = new THREE.WebGLRenderer({ antialias: true });

renderer.setSize(window.innerWidth, window.innerHeight);
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.setAnimationLoop(animate);
document.body.appendChild(renderer.domElement);

const controls = new OrbitControls(camera, renderer.domElement);
controls.target.set(0, 0, 0);
controls.update();

// --- Main Init ---
async function initScene() {
    try {
        // Objects
        const roomModel = await loader.loadAsync('/models/serverroom.glb');
        
        roomModel.scene.traverse((child) => {
            if ((child as THREE.Mesh).isMesh) {
                const m = child as THREE.Mesh;
                
                m.castShadow = true;
                m.receiveShadow = true;

                if ((m.material as THREE.MeshStandardMaterial).isMeshStandardMaterial) {
                    const mat = m.material as THREE.MeshStandardMaterial;
                    mat.metalness = 0;
                    mat.roughness = 1;
                }
            }
        });

        // Sensor positions
        for (let i = 0; i < sensor_names.length; i++) {
            const sensorObject = roomModel.scene.getObjectByName(sensor_names[i]);
            
            if (sensorObject) {
                console.log(`Found Sensor ${sensor_names[i]} at`, sensorObject.position);
                sensor_positions.push({ 
                    SensorId: sensor_names[i], 
                    Posiiton: sensorObject.position
                });
            } else {
                console.warn(`Sensor object '${sensor_names[i]}' not found in GLTF model.`);
            }
        }
        
        scene.add(roomModel.scene);

        // LIGHTS
        const ambient = new THREE.AmbientLight(0xffffff, 0.8);
        scene.add(ambient);

        const sun = new THREE.DirectionalLight(0xff9955, 5.0);
        sun.position.set(-50, 40, -5);
        sun.target = roomModel.scene;
        sun.castShadow = true;

        const d = 15; 
        sun.shadow.camera.left = -d;
        sun.shadow.camera.right = d;
        sun.shadow.camera.top = d;
        sun.shadow.camera.bottom = -d;

        sun.shadow.mapSize.width = 4096;
        sun.shadow.mapSize.height = 4096;

        scene.add(sun);

        const point1 = new THREE.PointLight(0xffffff, 7, 50);
        point1.position.set(-4, 4.5, 0);
        point1.castShadow = true;
        scene.add( point1 );

        const point2 = new THREE.PointLight(0xffffff, 7, 50);
        point2.position.set(4, 4.5, 0);
        point2.castShadow = true;
        scene.add( point2 );

        camera.position.set(0, 5, 10);

        build_heatmap(sensor_data);

    } catch (error) {
        console.error("Error loading scene:", error);
    }
}

// Start Init
initScene();

// --- Functions ---
function animate() {
    renderer.render(scene, camera);
}

window.addEventListener('resize', onWindowResize, false);

function onWindowResize() {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

function build_heatmap(data: SensorData[]) {
    const existingObject = scene.getObjectByName("VoxelGroup");
    if(existingObject) scene.remove(existingObject);

    const group = new THREE.Group();
    group.name = "VoxelGroup";

    const OFFSET_X = -7.5;
    const OFFSET_Z = -4.5;

    const xLimit = Math.floor(16 / VOXEL_DENSITY);
    const yLimit = Math.floor(6 / VOXEL_DENSITY);
    const zLimit = Math.floor(10 / VOXEL_DENSITY);

    const scale = 1;
    const cubeGeometry = new THREE.BoxGeometry(VOXEL_DENSITY * scale, VOXEL_DENSITY * scale, VOXEL_DENSITY * scale);

    for (let i = 0; i < xLimit; i++) {
        for (let j = 0; j < yLimit; j++) {
            for (let k = 0; k < zLimit; k++) {
                
                let positionX = VOXEL_DENSITY * i + OFFSET_X;
                let positionY = VOXEL_DENSITY * j + VOXEL_DENSITY / 2;
                let positionZ = VOXEL_DENSITY * k + OFFSET_Z;

                const positionV3 = new THREE.Vector3(positionX, positionY, positionZ);

                let totalWeight = 0;
                let weightedSum = 0;

                for (let s = 0; s < data.length; s++) {
                    const currentSensorData = data[s];
                    
                    const sensorPosObj = sensor_positions.find((val) => val.SensorId === currentSensorData.SensorId);
                    
                    if (!sensorPosObj) continue;

                    const dist = positionV3.distanceTo(sensorPosObj.Posiiton);
                    const weight = 1 / (Math.pow(dist, 2) + 0.0001);

                    const temp = currentSensorData.Temperature;
                    
                    // Null/Undefined Check für Temperatur
                    if (temp === null || temp === undefined) continue;

                    weightedSum += temp * weight;
                    totalWeight += weight;
                }

                if (totalWeight === 0) continue;

                const finalTemp = weightedSum / totalWeight;
                const color = getTemperatureColor(finalTemp);

                const material = new THREE.MeshBasicMaterial({
                    transparent: true,
                    opacity: 0.1,
                    color: color,
                    depthWrite: false,
                    depthTest: true
                });

                const mesh = new THREE.Mesh(cubeGeometry, material);
                
                mesh.position.copy(positionV3);
                group.add(mesh);
            }
        }
    }

    scene.add(group);
}

function getTemperatureColor(temp: number): THREE.Color {
    const ratio = Math.max(0, Math.min(1, (temp - COLD) / (WARM - COLD)));

    const hue = (1 - ratio) * 0.66;
    
    return new THREE.Color().setHSL(hue, 1, 0.5);
}