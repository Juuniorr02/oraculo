using Godot;
using System;
using System.IO;

public partial class NewProjectDialog : Window
{
    private LineEdit nameEdit;
    private LineEdit locationEdit;

    private Button browseButton;
    private Button cancelButton;
    private Button createButton;

    private FileDialog folderDialog;

    private ProjectManager projectManager;


    public override void _Ready()
    {
        nameEdit =
            GetNode<LineEdit>(
                "MarginContainer/VBoxContainer/NameEdit"
            );

        locationEdit =
            GetNode<LineEdit>(
                "MarginContainer/VBoxContainer/LocationContainer/LocationEdit"
            );

        browseButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/LocationContainer/BrowseButton"
            );

        cancelButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/CancelButton"
            );

        createButton =
            GetNode<Button>(
                "MarginContainer/VBoxContainer/Buttons/CreateButton"
            );

        projectManager =
            GetTree().Root.GetNode<ProjectManager>(
                "Main/ProjectManager"
            );


        browseButton.Pressed +=
            OnBrowsePressed;

        cancelButton.Pressed +=
            OnCancelPressed;

        createButton.Pressed +=
            OnCreatePressed;

        CloseRequested +=
            OnCancelPressed;


        locationEdit.Text =
            GetProjectsFolder();
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


    private void OnBrowsePressed()
    {
        if (folderDialog == null)
        {
            CreateFolderDialog();
        }

        folderDialog.CurrentDir =
            GetProjectsFolder();

        folderDialog.PopupCentered();
    }


    private void CreateFolderDialog()
    {
        folderDialog =
            new FileDialog();

        folderDialog.FileMode =
            FileDialog.FileModeEnum.OpenDir;

        folderDialog.Access =
            FileDialog.AccessEnum.Filesystem;

        folderDialog.Title =
            "Seleccionar ubicación";

        folderDialog.DirSelected +=
            OnFolderSelected;

        AddChild(
            folderDialog
        );
    }


    private void OnFolderSelected(
        string path)
    {
        locationEdit.Text =
            path;
    }


    private void OnCancelPressed()
    {
        Hide();
    }


    private void OnCreatePressed()
    {
        string projectName =
            nameEdit.Text.Trim();

        string folderPath =
            locationEdit.Text.Trim();


        if (string.IsNullOrWhiteSpace(projectName))
        {
            GD.PrintErr(
                "El nombre del proyecto está vacío."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(folderPath))
        {
            GD.PrintErr(
                "La ubicación del proyecto está vacía."
            );

            return;
        }


        string projectPath =
            Path.Combine(
                folderPath,
                projectName
            );


        if (Directory.Exists(projectPath))
        {
            GD.PrintErr(
                "Ya existe una carpeta con ese nombre."
            );

            return;
        }


        bool created =
            projectManager.CreateProject(
                projectPath,
                projectName
            );


        if (!created)
        {
            return;
        }


        Hide();
    }
}