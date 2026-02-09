import * as THREE from "three";
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { EffectComposer } from 'three/examples/jsm/postprocessing/EffectComposer.js';
import { RenderPass } from 'three/examples/jsm/postprocessing/RenderPass.js';
import { ShaderPass } from 'three/examples/jsm/postprocessing/ShaderPass.js';
import { HorizontalBlurShader } from 'three/examples/jsm/shaders/HorizontalBlurShader.js';
import { VerticalBlurShader } from 'three/examples/jsm/shaders/VerticalBlurShader.js';

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
let sensor_data: SensorData[] = [];
//     {
//         SensorId: "S1",
//         Temperature: 90 
//     },
//     {
//         SensorId: "S2",
//         Temperature: 90 
//     },
//     {
//         SensorId: "S3",
//         Temperature: 40 
//     },
//     {
//         SensorId: "S4",
//         Temperature: 40
//     },
//     {
//         SensorId: "SB",
//         Temperature: 30 
//     },
//     {
//         SensorId: "SD",
//         Temperature: 20 
//     },
// ];

const VOXEL_DENSITY = 0.25;
const SHADER_BLUR = 2;
const COLD = 20;
const WARM = 37;
const sensor_names: string[] = ["S1", "S2", "S3", "S4", "SB", "SD"];
let sensor_positions: SensorPos[] = [];

// --- WebView2 Listener ---
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', (event) => {
        console.log("Data Received:", event.data);
        sensor_data = event.data as SensorData[];

        build_heatmap(sensor_data);
    });
} else {
    console.error("Page was not loaded in WebView2 environment.");
}

// --- Three.js Setup ---
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
camera.layers.set(1);
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

// --- Composing ---
const renderScene = new RenderPass(scene, camera);

const blurComposer = new EffectComposer(renderer);
blurComposer.renderToScreen = false;
blurComposer.addPass(renderScene);

const hBlur = new ShaderPass(HorizontalBlurShader);
const vBlur = new ShaderPass(VerticalBlurShader);
hBlur.uniforms.h.value = 1 / window.innerWidth * SHADER_BLUR;
vBlur.uniforms.v.value = 1 / window.innerHeight * SHADER_BLUR;
blurComposer.addPass(hBlur);
blurComposer.addPass(vBlur);

const MixShader = {
    uniforms: {
        baseTexture: { value: null },
        blurTexture: { value: null }
    },
    vertexShader: `
        varying vec2 vUv;
        void main() {
            vUv = uv;
            gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
        }
    `,
    fragmentShader: `
        uniform sampler2D baseTexture;
        uniform sampler2D blurTexture;
        varying vec2 vUv;
        void main() {
            vec4 baseColor = texture2D( baseTexture, vUv );
            vec4 blurColor = texture2D( blurTexture, vUv );
            
            gl_FragColor = baseColor + (blurColor * 0.5);
        }
    `
};

const finalComposer = new EffectComposer(renderer);
finalComposer.addPass(renderScene);

const mixPass = new ShaderPass(MixShader, "baseTexture");
mixPass.needsSwap = true;
finalComposer.addPass(mixPass);

const maskMaterial = new THREE.MeshBasicMaterial({ color: 0x000000 });

// Start Init
initScene();

// --- Functions ---
function animate() {
    scene.traverse((obj) => {
        if(obj instanceof THREE.Mesh) {
            if (obj.layers.isEnabled(0)) {
                obj.userData.oldMat = obj.material;
                obj.material = maskMaterial;
            }
        }
    })

    camera.layers.enable(0);
    camera.layers.enable(1);

    blurComposer.render();
    // finalComposer.render();
    // return

    scene.traverse((obj) => {
        if(obj instanceof THREE.Mesh) {
            if (obj.isMesh && obj.userData.oldMat) {
                obj.material = obj.userData.oldMat;
                delete obj.userData.oldMat;
            }
        }
    })

    camera.layers.set(0);
    mixPass.uniforms.blurTexture.value = blurComposer.readBuffer.texture;

    finalComposer.render();
}

window.addEventListener('resize', onWindowResize, false);

function onWindowResize() {
    const width = window.innerWidth;
    const height = window.innerHeight;

    camera.aspect = width / height;
    camera.updateProjectionMatrix();

    renderer.setSize(width, height);

    blurComposer.setSize(width, height);
    finalComposer.setSize(width, height);

    if (hBlur.uniforms['h']) {
        hBlur.uniforms['h'].value = (1 / width) * SHADER_BLUR;
    }
    
    if (vBlur.uniforms['v']) {
        vBlur.uniforms['v'].value = (1 / height) * SHADER_BLUR;
    }
}

function build_heatmap(data: SensorData[]) {
    const existingObject = scene.getObjectByName("VoxelInstances");
    if(existingObject) scene.remove(existingObject);

    const OFFSET_X = -7.8;
    const OFFSET_Z = -5.25;

    const xLimit = Math.floor(16 / VOXEL_DENSITY);
    const yLimit = Math.floor(6 / VOXEL_DENSITY);
    const zLimit = Math.floor(10.75 / VOXEL_DENSITY);

    const scale = 1;
    const dummy = new THREE.Object3D();
    const cubeGeometry = new THREE.BoxGeometry(VOXEL_DENSITY * scale, VOXEL_DENSITY * scale, VOXEL_DENSITY * scale);
    const material = new THREE.MeshBasicMaterial({
        
    });

    const instancedMesh = new THREE.InstancedMesh(cubeGeometry, material, xLimit * yLimit * zLimit);
    instancedMesh.name = "VoxelInstances";
    instancedMesh.layers.set(1);
    scene.add(instancedMesh);

    let x = 0;
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
                    
                    if (temp === null || temp === undefined) continue;

                    weightedSum += temp * weight;
                    totalWeight += weight;
                }

                if (totalWeight === 0) continue;

                const finalTemp = weightedSum / totalWeight;
                const color = getTemperatureColor(finalTemp);

                dummy.position.copy(positionV3);
                dummy.updateMatrix();

                instancedMesh.setMatrixAt(x, dummy.matrix);
                instancedMesh.setColorAt(x, color);

                x++;
            }
        }
    }
}

function getTemperatureColor(temp: number): THREE.Color {
    const ratio = Math.max(0, Math.min(1, (temp - COLD) / (WARM - COLD)));

    const hue = (1 - ratio) * 0.66;
    
    return new THREE.Color().setHSL(hue, 1, 0.5);
}