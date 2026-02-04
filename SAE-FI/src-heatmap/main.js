import * as THREE from 'three';
import {OrbitControls} from 'three/addons/controls/OrbitControls.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';

let sensor_data = [];

if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', event => {
        console.log("Data Received:", event.data);
        sensor_data = event.data;
    });
} else {
    console.error("Page was not loaded in WebView2 environment.");
}

const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera( 75, window.innerWidth / window.innerHeight, 0.1, 1000 );
const loader = new GLTFLoader();

const VOXEL_DENSITY = 0.75;
const COLD = 20;
const WARM = 80;

let sensor_positions = [
    {
        sId: "S1",
        position: undefined
    },
    {
        sId: "S2",
        position: undefined
    },
    {
        sId: "S3",
        position: undefined
    },
    {
        sId: "S4",
        position: undefined
    },
    {
        sId: "SB",
        position: undefined
    },
    {
        sId: "SD",
        position: undefined
    },
]

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize( window.innerWidth, window.innerHeight );
renderer.setAnimationLoop( animate );
document.body.appendChild( renderer.domElement );

const controls = new OrbitControls(camera, renderer.domElement);
controls.target.set(0, 0, 0);
controls.update();

// OBJECTS
const roomModel = await loader.loadAsync('/models/serverroom.glb');
for (let i = 0; i < sensor_positions.length; i++) {
    const sensorObject = roomModel.scene.getObjectByName(sensor_positions[i].sId);
    console.log(sensorObject.position);
    sensor_positions[i].position = sensorObject.position;
}
scene.add(roomModel.scene);

// LIGHTS
var ambient = new THREE.AmbientLight(0xffffff, 1);
scene.add( ambient );

var sun = new THREE.DirectionalLight(new THREE.Color().setRGB(1.5, 1, 1), 1.0);
sun.position.set(1,2,1);
sun.target = roomModel.scene;
scene.add( sun );


camera.position.set(0,5,10);

build_voxel_grid(sensor_data);

function animate() {
  scene.traverse(function(object){
			object.castShadow = true
			object.receiveShadow = true
		});

  renderer.render( scene, camera );

}

window.addEventListener( 'resize', onWindowResize, false );

function onWindowResize(){

    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();

    renderer.setSize( window.innerWidth, window.innerHeight );

}

function build_voxel_grid(data) {
    let group = new THREE.Group();
    const OFFSET_X = -7.5;
    const OFFSET_Z = -5.5;

    for (let i = 0; i < Math.floor(16 / VOXEL_DENSITY); i++) {
        for (let j = 0; j < Math.floor(8 / VOXEL_DENSITY); j++) {
            for (let k = 0; k < Math.floor(12 / VOXEL_DENSITY); k++) {
                let cube = new THREE.BoxGeometry(VOXEL_DENSITY, VOXEL_DENSITY, VOXEL_DENSITY);

                let positionX = VOXEL_DENSITY * i + OFFSET_X;
                let positionY = VOXEL_DENSITY * j + VOXEL_DENSITY  / 2;
                let positionZ = VOXEL_DENSITY * k + OFFSET_Z;

                const positionV3 = new THREE.Vector3(positionX, positionY, positionZ);

                let totalWeight = 0;
                let weightedSum = 0;

                for (let s = 0; s < sensor_data.length; s++) {
                    let sensor = sensor_positions.find((val) => val.sId === sensor_data[s]["SensorId"]);
                    if (!sensor) continue;

                    const sensorV3 = new THREE.Vector3(sensor.position.x, sensor.position.y, sensor.position.z);
                    const dist = positionV3.distanceTo(sensorV3);

                    const weight = 1 / (Math.pow(dist, 2) + 0.0001);
                    
                    let temp = sensor_data.find((el) => el["SensorId"] === sensor.sId)["Temperature"];
                    if (temp == undefined) continue;

                    weightedSum += temp * weight;
                    totalWeight += weight;
                }

                const finalTemp = weightedSum / totalWeight;
                const color = getTemperatureColor(finalTemp);

                let mesh = new THREE.Mesh(cube, new THREE.MeshBasicMaterial(
                    {
                        transparent: true,
                        opacity: 0.1,
                        color: color,
                        depthWrite: false,
                        depthTest: true
                    }
                ));
        
                mesh.position.set(
                    positionX,
                    positionY,
                    positionZ
                );
        
                group.add(mesh);
            }
        }
    }

    scene.add(group);
}

function getTemperatureColor(temp) {
    const ratio = Math.max(0, Math.min(1, (temp - COLD) / (WARM - COLD)));
    
    const hue = (1 - ratio) * 0.66; 
    return new THREE.Color().setHSL(hue, 1, 0.5);
}