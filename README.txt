git lfs install
git lfs track "*.png" "*.jpg" "*.psd" "*.tga" "*.wav" "*.mp3" "*.mp4" "*.mov" "*.fbx" "*.glb" "*.gltf" "*.anim" "*.prefab" "*.unitypackage"
git add .gitattributes
git commit -m "Configure Git LFS for large Unity assets"
git push

