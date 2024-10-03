using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

using static MvVox.VoxCreater;


namespace MvVox
{
    [CustomEditor(typeof(VoxCreater))]
    public class VoxCreaterInspector : Editor
    {
        private VoxCreater _t;
        private VoxCreater _target
        {
            get
            {
                if (_t == null)
                {
                    _t = (VoxCreater)target;
                }
                return _t;
            }
        }

        private NetDrawer _netDrawer;
        private CameraObseverer _cameraObserver;
        private VoxelInteractionHandler _interactionHandler;
        private InputManager _inputManager;
        private VoxelStorage _voxelStorage;
        private VoxelDrawer _voxelDrawer;
        private ProjectionManager _projectionManager;
        private EditorBrushModeHandler _editroBrushModeHandler;
        private RaycastManager _raycastManager;


        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            VoxCreater voxCreater = (VoxCreater)target;

            var nameField = new TextField("Name of Object");
            nameField.BindProperty(serializedObject.FindProperty("Name"));
            root.Add(nameField);

            var voxelSizeField = new FloatField("Voxel Size");
            voxelSizeField.BindProperty(serializedObject.FindProperty("VoxelSize")); 
            root.Add(voxelSizeField);

            var netSizeField = new Vector3IntField("Net Size");
            netSizeField.BindProperty(serializedObject.FindProperty("_netSize"));
            netSizeField.RegisterValueChangedCallback((evt) =>
            {
                Undo.RecordObject(voxCreater, "Resize");
                voxCreater.Resize();
                EditorUtility.SetDirty(voxCreater);
            });
            root.Add(netSizeField);

            var netOffsetField = new Vector3Field("Net Offset");
            netOffsetField.BindProperty(serializedObject.FindProperty("netOffset"));
            root.Add(netOffsetField);

            var brushColorField = new ColorField("Brush Color");
            brushColorField.BindProperty(serializedObject.FindProperty("DrawColor"));
            root.Add(brushColorField);

            Label brushTypeLabel = new Label("Brush Type: None");
            brushTypeLabel.style.marginTop = 10;
            brushTypeLabel.style.textOverflow = TextOverflow.Ellipsis;
            brushTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            root.Add(brushTypeLabel);

            var buttonContainer = new VisualElement();

            buttonContainer.style.borderBottomWidth = 1;
            buttonContainer.style.borderTopWidth = 1;
            buttonContainer.style.borderLeftWidth = 1;
            buttonContainer.style.borderRightWidth = 1;

            buttonContainer.style.borderBottomColor = Color.gray;
            buttonContainer.style.borderTopColor = Color.gray;
            buttonContainer.style.borderLeftColor = Color.gray;
            buttonContainer.style.borderRightColor = Color.gray;

            buttonContainer.style.borderBottomLeftRadius = 5;
            buttonContainer.style.borderBottomRightRadius = 5;
            buttonContainer.style.borderTopLeftRadius = 5;
            buttonContainer.style.borderTopRightRadius = 5;

            buttonContainer.style.marginTop = 10;
            buttonContainer.style.marginBottom = 10;
            buttonContainer.style.paddingTop = 5;
            buttonContainer.style.paddingBottom = 5;

            var insfrastructureContainer = new VisualElement();
            insfrastructureContainer.style.flexDirection = FlexDirection.Row;
            insfrastructureContainer.style.justifyContent = Justify.Center;
            Button clearButton = new Button(() =>
            {
                Undo.RecordObject(voxCreater, "Clear");
                voxCreater.Clear();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(voxCreater);
            }) { text = "Clear" };

            Button setNetCenter = new Button(() =>
            {
                Undo.RecordObject(voxCreater, "Center Voxel Net");
                voxCreater.PutNetAtCenter();
                EditorUtility.SetDirty(voxCreater);
            }){ text = "Put Net At Center" };

            clearButton.style.flexGrow = 1;
            setNetCenter.style.flexGrow = 1;

            insfrastructureContainer.Add(clearButton);
            insfrastructureContainer.Add(setNetCenter);

            buttonContainer.Add(insfrastructureContainer);

            var brushToolsContainer = new VisualElement();
            brushToolsContainer.style.flexDirection = FlexDirection.Row;
            brushToolsContainer.style.justifyContent = Justify.SpaceBetween;
            brushToolsContainer.style.marginTop = 5;

            var noneButton = new Button(() => SetBrushMode(BrushMode.None, brushTypeLabel)) { text = "None" };
            var paintButton = new Button(() => SetBrushMode(BrushMode.Paint, brushTypeLabel)) { text = "Paint" };
            var eraseButton = new Button(() => SetBrushMode(BrushMode.Erase, brushTypeLabel)) { text = "Erase" };
            var pipetteButton = new Button(() => SetBrushMode(BrushMode.Pippette, brushTypeLabel)) { text = "Pipette" };

            noneButton.style.flexGrow = 1;
            paintButton.style.flexGrow = 1;
            eraseButton.style.flexGrow = 1;
            pipetteButton.style.flexGrow = 1;

            brushToolsContainer.Add(noneButton);
            brushToolsContainer.Add(paintButton);
            brushToolsContainer.Add(eraseButton);
            brushToolsContainer.Add(pipetteButton);

            var generateContainer = new VisualElement();
            generateContainer.style.marginTop = 5;

            Button generatiBtn = new Button(() => {
                Undo.RecordObject(voxCreater, "Generate GameObject");
                _target.GenerateMesh();
                EditorUtility.SetDirty(voxCreater);
            }) { text = "Generate Mesh" };
            generateContainer.Add(generatiBtn);

            buttonContainer.Add(brushToolsContainer); 
            buttonContainer.Add(generateContainer);

            root.Add(buttonContainer); 

            return root;
        }

        private void SetBrushMode(BrushMode mode, Label brushTypeLabel = null)
        {
            var voxCreater = (VoxCreater)target;
            var property = serializedObject.FindProperty("CurrentBrushMode");
            property.enumValueIndex = (int)mode;
            _editroBrushModeHandler?.SetBrushMode(mode);
            if (brushTypeLabel != null)
                brushTypeLabel.text = $"Brush Type: {mode}";

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _editroBrushModeHandler = new EditorBrushModeHandler(_target);
            _inputManager = new InputManager();
            _raycastManager = new RaycastManager(_target);
            _cameraObserver = new CameraObseverer(_target);
            _netDrawer = new NetDrawer(_target);
            _voxelStorage = new VoxelStorage(_target);
            _voxelDrawer = new VoxelDrawer(_voxelStorage);
            _projectionManager = new ProjectionManager(_target, _voxelStorage);
            _interactionHandler = new VoxelInteractionHandler(_target, _voxelStorage);

            Initialize();
        }

        private void Initialize()
        {
            _editroBrushModeHandler.OnBrushModeChanged += _raycastManager.OnBrushModeChanged;
            _editroBrushModeHandler.OnBrushModeChanged += _interactionHandler.OnBrushModeChanged;

            _cameraObserver.OnCameraChanged += _netDrawer.UpdateCameraData;
            _cameraObserver.OnCameraChanged += _raycastManager.UpdateVisiblePlanes;

            _inputManager.OnMousePositionChanged += _raycastManager.HandleInteraction;
            _inputManager.OnMouse0Down += (pos) =>
            {
                _interactionHandler.HandleMouseDown(pos);
                _inputManager.Use();
            };

            _raycastManager.OnHoweredVoxel += _projectionManager.DrawFromHoveredPosition;
            _raycastManager.OnHoweredVoxel += _interactionHandler.HandleHoweredVoxel; 

            _editroBrushModeHandler.Initialize();
        }

        private void OnSceneGUI()
        {
            if (_target.IsDrawNet)
            {
                _inputManager.Update(Event.current);
                _cameraObserver.Observe();
                _netDrawer.DrawInsideCubeFaces();
                _voxelDrawer.DrawVoxels(_target.VoxelSize);
                _projectionManager.DrawOutlined();
                _raycastManager.HandleInteraction(_inputManager.MousePosition); 
            }
        }

        #region DebugMethods
        private void VisualizationWorkspase()
        {
            Handles.color = Color.yellow;
            Vector3 cubeSize = new Vector3(_target.NetSize.x, _target.NetSize.y, _target.NetSize.z) * _target.VoxelSize;
            Handles.DrawWireCube(_target.NetPosition + cubeSize * 0.5f, cubeSize);
        } 
        #endregion
    }
}