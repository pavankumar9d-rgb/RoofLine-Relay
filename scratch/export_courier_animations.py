import bpy
import os

src_dir = r"C:\Users\saipa\Downloads\animations\01_courier_female_urban_scout"
dest_char = r"C:\Users\saipa\OneDrive\Desktop\Roofline Relay\unity-game\Assets\Art\Characters"
dest_anim = r"C:\Users\saipa\OneDrive\Desktop\Roofline Relay\unity-game\Assets\Art\Animations\CourierFemale"

os.makedirs(dest_char, exist_ok=True)
os.makedirs(dest_anim, exist_ok=True)

# 1. Base Character Model (Skinned Mesh + Armature in T-pose, with textures)
base_src = os.path.join(src_dir, "Idle.fbx")
base_dest = os.path.join(dest_char, "courier_female_urban_scout_rigged.fbx")

print("Exporting Base Character Model in T-pose...")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=base_src)

# Remove Camera and Light if any
for o in [o for o in bpy.data.objects if o.type in ('CAMERA', 'LIGHT')]:
    bpy.data.objects.remove(o, do_unlink=True)

arms = [o for o in bpy.data.objects if o.type == 'ARMATURE']
if arms:
    arm = arms[0]
    arm.data.pose_position = 'REST'
    if arm.animation_data:
        arm.animation_data_clear()

# Clear all actions from base model
for a in list(bpy.data.actions):
    bpy.data.actions.remove(a, do_unlink=True)

bpy.context.view_layer.update()

bpy.ops.export_scene.fbx(
    filepath=base_dest,
    use_selection=False,
    bake_anim=False,
    embed_textures=True,
    path_mode='COPY',
    mesh_smooth_type='FACE'
)
print(f"Base model exported: {base_dest} ({os.path.getsize(base_dest) / (1024*1024):.2f} MB)")

# 2. Animation Clips (Armature + Animation curves only, no redundant mesh)
clips = [
    ("Idle.fbx", "UrbanScout_Idle.fbx"),
    ("Walk.fbx", "UrbanScout_Walk.fbx"),
    ("Jog Forward.fbx", "UrbanScout_Jog.fbx"),
    ("Fast Run.fbx", "UrbanScout_Run.fbx"),
    ("Jump.fbx", "UrbanScout_Jump.fbx"),
    ("Falling Down.fbx", "UrbanScout_Falling.fbx"),
    ("Hard Landing.fbx", "UrbanScout_Landing.fbx"),
    ("vault.fbx", "UrbanScout_Vault.fbx")
]

print("\nExporting Animation-Only FBX Clips...")
for src_name, dest_name in clips:
    src_path = os.path.join(src_dir, src_name)
    out_path = os.path.join(dest_anim, dest_name)
    
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=src_path)
    
    # Remove Camera, Light, and Meshes (keep ONLY Armature with keyframes)
    for o in [o for o in bpy.data.objects if o.type in ('CAMERA', 'LIGHT', 'MESH')]:
        bpy.data.objects.remove(o, do_unlink=True)
        
    bpy.ops.export_scene.fbx(
        filepath=out_path,
        use_selection=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=True,
        bake_anim_use_all_actions=True,
        add_leaf_bones=False
    )
    print(f"  {dest_name:25} -> ({os.path.getsize(out_path)/1024:.1f} KB)")

print("\nAll 8 animation clips exported successfully.")
