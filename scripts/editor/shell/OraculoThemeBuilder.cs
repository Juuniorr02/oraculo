using Godot;

public static class OraculoThemeBuilder
{
    public static Theme CreateTheme()
    {
        Theme theme =
            new Theme();


        // =====================================================
        // TAMAÑO DE FUENTE GLOBAL
        // =====================================================

        theme.DefaultFontSize =
            15;


        // =====================================================
        // TAMAÑOS DE FUENTE
        // =====================================================

        theme.SetFontSize(
            "font_size",
            "Label",
            15
        );


        theme.SetFontSize(
            "font_size",
            "Button",
            15
        );


        theme.SetFontSize(
            "font_size",
            "CheckButton",
            15
        );


        theme.SetFontSize(
            "font_size",
            "LineEdit",
            15
        );


        return theme;
    }
}