using Godot;
using System.IO;

public partial class StartScreen : Control
{
    private Button newProjectButton;
    private Button openProjectButton;

    private PackedScene newProjectDialogScene;

    private FileDialog openProjectDialog;

    private ProjectManager projectManager;


    public override void _Ready()
    {
        newProjectButton =
            GetNode<Button>(
                "CenterContainer/VBoxContainer/NewProjectButton"
            );

        openProjectButton =
            GetNode<Button>(
                "CenterContainer/VBoxContainer/OpenProjectButton"
            );

        projectManager =
            GetTree().Root.GetNode<ProjectManager>(
                "Main/ProjectManager"
            );

        newProjectButton.Pressed +=
            OnNewProjectPressed;

        openProjectButton.Pressed +=
            OnOpenProjectPressed;

        newProjectDialogScene =
            GD.Load<PackedScene>(
                "res://scenes/NewProjectDialog.tscn"
            );
    }


    private string GetProjectsFolder()
    {
        string documentsFolder =
            System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.MyDocuments
            );

        string projectsFolder =
            Path.Combine(
                documentsFolder,
                "Pruebas Oraculo"
            );

        Directory.CreateDirectory(
            projectsFolder
        );

        return projectsFolder;
    }


    private void OnNewProjectPressed()
    {
        NewProjectDialog dialog =
            newProjectDialogScene.Instantiate<NewProjectDialog>();

        AddChild(
            dialog
        );

        dialog.PopupCentered();
    }


    private void OnOpenProjectPressed()
    {
        if (openProjectDialog == null)
        {
            CreateOpenProjectDialog();
        }

        openProjectDialog.CurrentDir =
            GetProjectsFolder();

        openProjectDialog.PopupCentered();
    }


    private void CreateOpenProjectDialog()
    {
        openProjectDialog =
            new FileDialog();

        openProjectDialog.FileMode =
            FileDialog.FileModeEnum.OpenDir;

        openProjectDialog.Access =
            FileDialog.AccessEnum.Filesystem;

        openProjectDialog.Title =
            "Abrir proyecto";

        openProjectDialog.DirSelected +=
            OnProjectFolderSelected;

        AddChild(
            openProjectDialog
        );
    }


    private void OnProjectFolderSelected(
        string folderPath)
    {
        string projectFilePath =
            Path.Combine(
                folderPath,
                "project.json"
            );

        if (!File.Exists(projectFilePath))
        {
            GD.PrintErr(
                "La carpeta seleccionada no contiene un proyecto de Oráculo."
            );

            return;
        }

        bool loaded =
            projectManager.LoadProject(
                folderPath
            );

        if (!loaded)
        {
            GD.PrintErr(
                "No se pudo abrir el proyecto."
            );

            return;
        }

        GD.Print(
            "Proyecto abierto correctamente: ",
            folderPath
        );
    }
}