using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class ProjectManager : Node
{
    // =========================================================
    // SEÑALES
    // =========================================================

    [Signal]
    public delegate void ProjectOpenedEventHandler();

    [Signal]
    public delegate void ProjectClosedEventHandler();

    [Signal]
    public delegate void ProjectSavedEventHandler();

    [Signal]
    public delegate void ProjectModifiedEventHandler();


    // =========================================================
    // PROYECTO ACTUAL
    // =========================================================

    private ProjectData currentProject;

    private string currentProjectPath = "";

    private bool hasUnsavedChanges = false;


    // =========================================================
    // ESTADO
    // =========================================================

    public bool HasProject()
    {
        return currentProject != null;
    }


    public ProjectData GetCurrentProject()
    {
        return currentProject;
    }


    public string GetProjectPath()
    {
        return currentProjectPath;
    }


    public bool HasUnsavedChanges()
    {
        return hasUnsavedChanges;
    }


    // =========================================================
    // CREAR PROYECTO
    // =========================================================

    public bool CreateProject(
        string folderPath,
        string projectName)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            GD.PrintErr(
                "ProjectManager: la ruta del proyecto está vacía."
            );

            return false;
        }


        if (string.IsNullOrWhiteSpace(projectName))
        {
            GD.PrintErr(
                "ProjectManager: el nombre del proyecto está vacío."
            );

            return false;
        }


        try
        {
            Directory.CreateDirectory(
                folderPath
            );


            CreateProjectFolders(
                folderPath
            );


            currentProject =
                new ProjectData
                {
                    Name = projectName,
                    Version = 1
                };


            currentProjectPath =
                folderPath;


            hasUnsavedChanges =
                true;


            bool saved =
                SaveProject();


            if (!saved)
            {
                currentProject = null;
                currentProjectPath = "";
                hasUnsavedChanges = false;

                return false;
            }


            // =====================================================
            // DATOS INICIALES
            // =====================================================

            AttributeDataGenerator.Generate(
                currentProjectPath
            );


            ChapterDataGenerator.Generate(
                currentProjectPath
            );


            EmitSignal(
                SignalName.ProjectOpened
            );


            GD.Print(
                "Proyecto creado: ",
                projectName
            );


            return true;
        }
        catch (Exception exception)
        {
            GD.PrintErr(
                "Error creando el proyecto: ",
                exception.Message
            );

            return false;
        }
    }


    // =========================================================
    // GUARDAR PROYECTO
    // =========================================================

    public bool SaveProject()
    {
        if (currentProject == null)
        {
            GD.PrintErr(
                "ProjectManager: no hay ningún proyecto cargado."
            );

            return false;
        }


        if (string.IsNullOrEmpty(currentProjectPath))
        {
            GD.PrintErr(
                "ProjectManager: el proyecto no tiene una ruta."
            );

            return false;
        }


        try
        {
            string projectFilePath =
                Path.Combine(
                    currentProjectPath,
                    "project.json"
                );


            JsonSerializerOptions options =
                new JsonSerializerOptions
                {
                    WriteIndented = true
                };


            string json =
                JsonSerializer.Serialize(
                    currentProject,
                    options
                );


            File.WriteAllText(
                projectFilePath,
                json
            );


            hasUnsavedChanges =
                false;


            EmitSignal(
                SignalName.ProjectSaved
            );


            GD.Print(
                "Proyecto guardado: ",
                projectFilePath
            );


            return true;
        }
        catch (Exception exception)
        {
            GD.PrintErr(
                "Error guardando el proyecto: ",
                exception.Message
            );

            return false;
        }
    }


    // =========================================================
    // CARGAR PROYECTO
    // =========================================================

    public bool LoadProject(
        string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return false;
        }


        string projectFilePath =
            Path.Combine(
                folderPath,
                "project.json"
            );


        if (!File.Exists(projectFilePath))
        {
            GD.PrintErr(
                "No se encontró project.json."
            );

            return false;
        }


        try
        {
            string json =
                File.ReadAllText(
                    projectFilePath
                );


            ProjectData loadedProject =
                JsonSerializer.Deserialize<ProjectData>(
                    json
                );


            if (loadedProject == null)
            {
                GD.PrintErr(
                    "El proyecto no pudo ser cargado."
                );

                return false;
            }


            currentProject =
                loadedProject;


            currentProjectPath =
                folderPath;


            hasUnsavedChanges =
                false;


            EmitSignal(
                SignalName.ProjectOpened
            );


            GD.Print(
                "Proyecto cargado: ",
                currentProject.Name
            );


            return true;
        }
        catch (Exception exception)
        {
            GD.PrintErr(
                "Error cargando el proyecto: ",
                exception.Message
            );

            return false;
        }
    }


    // =========================================================
    // MARCAR CAMBIOS
    // =========================================================

    public void MarkAsModified()
    {
        if (currentProject == null)
        {
            return;
        }


        if (hasUnsavedChanges)
        {
            return;
        }


        hasUnsavedChanges =
            true;


        EmitSignal(
            SignalName.ProjectModified
        );
    }


    // =========================================================
    // CERRAR PROYECTO
    // =========================================================

    public bool CloseProject()
    {
        if (!HasProject())
        {
            return true;
        }


        if (hasUnsavedChanges)
        {
            GD.PrintErr(
                "ProjectManager: el proyecto tiene cambios sin guardar."
            );

            return false;
        }


        currentProject =
            null;


        currentProjectPath =
            "";


        hasUnsavedChanges =
            false;


        EmitSignal(
            SignalName.ProjectClosed
        );


        return true;
    }


    // =========================================================
    // CREAR ESTRUCTURA
    // =========================================================

    private void CreateProjectFolders(
        string projectFolder)
    {
        string[] folders =
        {
            "events",
            "interludes",
            "chapters",
            "characters",
            "attributes",
            "relationships",
            "localization"
        };


        foreach (string folder in folders)
        {
            Directory.CreateDirectory(
                Path.Combine(
                    projectFolder,
                    folder
                )
            );
        }
    }
}