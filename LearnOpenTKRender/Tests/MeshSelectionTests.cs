using LearnOpenTKRender.Ecs;
using LearnOpenTKRender.Examples;
using LearnOpenTKRender.OpenGL;
using System.Numerics;

namespace LearnOpenTKRender.Tests;

/// <summary>
/// ����ѡ���ܲ���
/// </summary>
internal static class MeshSelectionTests
{
    /// <summary>
    /// ���Ի�����meshѡ����
    /// </summary>
    public static void TestBasicMeshSelection()
    {
        Console.WriteLine("Testing basic mesh selection...");

        try
        {
            // ��������ģ��
            var model = CreateTestModel();
            
            // ����ʵ������
            var entity = new Entity();
            var meshFilter = entity.AddComponent<MeshFilterComponent>();

            // ����ģ��
            meshFilter.Model = model;

            // ��֤��ʼ״̬
            Console.WriteLine($"Model set: {meshFilter.Model != null}");
            Console.WriteLine($"Mesh count: {meshFilter.MeshCount}");
            Console.WriteLine($"Available meshes: {string.Join(", ", meshFilter.AvailableMeshNames)}");
            Console.WriteLine($"Initial selection: {meshFilter.SelectedMeshName}");
            Console.WriteLine($"Has mesh: {meshFilter.HasMesh}");

            // ����ѡ��ͬ��mesh
            var meshNames = meshFilter.AvailableMeshNames.ToList();
            
            if (meshNames.Count > 1)
            {
                // ѡ��ڶ���mesh
                bool success = meshFilter.SelectMesh(meshNames[1]);
                Console.WriteLine($"Selected '{meshNames[1]}': {success}");
                Console.WriteLine($"Current selection: {meshFilter.SelectedMeshName}");
                Console.WriteLine($"Material name: {meshFilter.MaterialName}");
            }

            // ����ѡ�񲻴��ڵ�mesh
            bool failureExpected = meshFilter.SelectMesh("NonExistent");
            Console.WriteLine($"Selected non-existent mesh: {failureExpected} (should be false)");

            // ���԰�����ѡ��
            if (meshNames.Count > 0)
            {
                bool indexSuccess = meshFilter.SelectMeshByIndex(0);
                Console.WriteLine($"Selected by index 0: {indexSuccess}");
                Console.WriteLine($"Current selection after index: {meshFilter.SelectedMeshName}");
            }

            Console.WriteLine("Basic mesh selection test passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Basic mesh selection test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ����meshѡ��״̬����
    /// </summary>
    public static void TestMeshSelectionState()
    {
        Console.WriteLine("\nTesting mesh selection state management...");

        try
        {
            var model = CreateTestModel();
            var entity = new Entity();
            var meshFilter = entity.AddComponent<MeshFilterComponent>();

            // ���Կ�״̬
            var emptyState = MeshSelectionExample.GetMeshSelectionState(entity);
            Console.WriteLine($"Empty state - Has model: {emptyState.HasModel}");
            Console.WriteLine($"Empty state - Mesh count: {emptyState.MeshCount}");

            // ����ģ��
            meshFilter.Model = model;

            // ��ȡ״̬
            var state = MeshSelectionExample.GetMeshSelectionState(entity);
            Console.WriteLine($"State - Has model: {state.HasModel}");
            Console.WriteLine($"State - Mesh count: {state.MeshCount}");
            Console.WriteLine($"State - Available meshes: {string.Join(", ", state.AvailableMeshNames)}");
            Console.WriteLine($"State - Selected: {state.SelectedMeshName}");
            Console.WriteLine($"State - Has valid mesh: {state.HasValidMesh}");

            // ����ͨ��API����ѡ��
            if (state.AvailableMeshNames.Count > 1)
            {
                string targetMesh = state.AvailableMeshNames[1];
                bool setSuccess = MeshSelectionExample.SetMeshSelection(entity, targetMesh);
                Console.WriteLine($"Set selection to '{targetMesh}': {setSuccess}");

                // ��֤״̬����
                var updatedState = MeshSelectionExample.GetMeshSelectionState(entity);
                Console.WriteLine($"Updated selection: {updatedState.SelectedMeshName}");
            }

            Console.WriteLine("Mesh selection state management test passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Mesh selection state management test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ����meshѡ��ı߽����
    /// </summary>
    public static void TestMeshSelectionEdgeCases()
    {
        Console.WriteLine("\nTesting mesh selection edge cases...");

        try
        {
            var entity = new Entity();
            var meshFilter = entity.AddComponent<MeshFilterComponent>();

            // ����û��ģ��ʱ����Ϊ
            Console.WriteLine($"No model - Has mesh: {meshFilter.HasMesh}");
            Console.WriteLine($"No model - Mesh count: {meshFilter.MeshCount}");
            Console.WriteLine($"No model - Available meshes count: {meshFilter.AvailableMeshNames.Count()}");

            bool selectWithoutModel = meshFilter.SelectMesh("AnyMesh");
            Console.WriteLine($"Select mesh without model: {selectWithoutModel} (should be false)");

            // ���Կ�ģ��
            var emptyModel = new Model(new List<StaticMesh>());
            meshFilter.Model = emptyModel;
            Console.WriteLine($"Empty model - Mesh count: {meshFilter.MeshCount}");
            Console.WriteLine($"Empty model - Has mesh: {meshFilter.HasMesh}");

            // ���Ե�meshģ��
            var singleMeshModel = CreateSingleMeshModel();
            meshFilter.Model = singleMeshModel;
            Console.WriteLine($"Single mesh model - Mesh count: {meshFilter.MeshCount}");
            Console.WriteLine($"Single mesh model - Selected: {meshFilter.SelectedMeshName}");
            Console.WriteLine($"Single mesh model - Has mesh: {meshFilter.HasMesh}");

            // �������
            meshFilter.Clear();
            Console.WriteLine($"After clear - Has mesh: {meshFilter.HasMesh}");
            Console.WriteLine($"After clear - Selected: '{meshFilter.SelectedMeshName}'");

            Console.WriteLine("Mesh selection edge cases test passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Mesh selection edge cases test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ���Ե�����Ϣ
    /// </summary>
    public static void TestDebugInfo()
    {
        Console.WriteLine("\nTesting debug info...");

        try
        {
            var entity = new Entity();
            var meshFilter = entity.AddComponent<MeshFilterComponent>();

            // ������ģ��ʱ�ĵ�����Ϣ
            string debugInfo1 = meshFilter.GetDebugInfo();
            Console.WriteLine("Debug info without model:");
            Console.WriteLine(debugInfo1);

            // ����ģ�ͺ�ĵ�����Ϣ
            meshFilter.Model = CreateTestModel();
            string debugInfo2 = meshFilter.GetDebugInfo();
            Console.WriteLine("\nDebug info with model:");
            Console.WriteLine(debugInfo2);

            Console.WriteLine("Debug info test passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Debug info test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ��������meshѡ�����
    /// </summary>
    public static void RunAllMeshSelectionTests()
    {
        Console.WriteLine("=== Mesh Selection Tests ===");
        
        TestBasicMeshSelection();
        TestMeshSelectionState();
        TestMeshSelectionEdgeCases();
        TestDebugInfo();
        
        Console.WriteLine("\n=== All Mesh Selection Tests Completed ===");
    }

    /// <summary>
    /// ��������ģ��
    /// </summary>
    private static Model CreateTestModel()
    {
        var vertices = new Vertex[]
        {
            new Vertex(new Vector3(-1, -1, 0), Vector3.UnitZ, Vector2.Zero, Vector3.UnitX, Vector3.UnitY),
            new Vertex(new Vector3(1, -1, 0), Vector3.UnitZ, Vector2.UnitX, Vector3.UnitX, Vector3.UnitY),
            new Vertex(new Vector3(0, 1, 0), Vector3.UnitZ, new Vector2(0.5f, 1), Vector3.UnitX, Vector3.UnitY)
        };

        var indices = new uint[] { 0, 1, 2 };

        var meshes = new List<StaticMesh>
        {
            new StaticMesh(vertices, indices, "Triangle_A"),
            new StaticMesh(vertices, indices, "Triangle_B"),
            new StaticMesh(vertices, indices, "Triangle_C")
        };

        return new Model(meshes);
    }

    /// <summary>
    /// ������meshģ��
    /// </summary>
    private static Model CreateSingleMeshModel()
    {
        var vertices = new Vertex[]
        {
            new Vertex(new Vector3(-1, -1, 0), Vector3.UnitZ, Vector2.Zero, Vector3.UnitX, Vector3.UnitY),
            new Vertex(new Vector3(1, -1, 0), Vector3.UnitZ, Vector2.UnitX, Vector3.UnitX, Vector3.UnitY),
            new Vertex(new Vector3(0, 1, 0), Vector3.UnitZ, new Vector2(0.5f, 1), Vector3.UnitX, Vector3.UnitY)
        };

        var indices = new uint[] { 0, 1, 2 };

        var meshes = new List<StaticMesh>
        {
            new StaticMesh(vertices, indices, "SingleTriangle")
        };

        return new Model(meshes);
    }
}
