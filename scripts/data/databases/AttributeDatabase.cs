using System.Collections.Generic;

public class AttributeDatabase
{
    private AttributeRepository attributeRepository;

    private ChapterRepository chapterRepository;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public AttributeDatabase()
    {
    }


    public AttributeDatabase(
        string projectFolder)
    {
        Initialize(
            projectFolder
        );
    }


    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    public void Initialize(
        string projectFolder)
    {
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            attributeRepository = null;
            chapterRepository = null;

            return;
        }


        attributeRepository =
            new AttributeRepository(
                projectFolder
            );


        chapterRepository =
            new ChapterRepository(
                projectFolder
            );
    }


    // =========================================================
    // TODOS LOS ATRIBUTOS
    // =========================================================

    public List<AttributeDefinitionData> GetAllAttributes()
    {
        if (attributeRepository == null)
        {
            return new List<AttributeDefinitionData>();
        }


        return attributeRepository.LoadAll();
    }


    // =========================================================
    // ATRIBUTO POR ID
    // =========================================================

    public AttributeDefinitionData GetAttribute(
        string id)
    {
        if (attributeRepository == null)
        {
            return null;
        }


        return attributeRepository.Load(
            id
        );
    }


    // =========================================================
    // ATRIBUTOS DE UN CAPÍTULO
    // =========================================================

    public List<AttributeDefinitionData> GetAttributesForChapter(
        int chapterNumber)
    {
        List<AttributeDefinitionData> result =
            new List<AttributeDefinitionData>();


        if (attributeRepository == null ||
            chapterRepository == null)
        {
            return result;
        }


        List<ChapterDefinitionData> chapters =
            chapterRepository.LoadAll();


        foreach (
            ChapterDefinitionData chapter
            in chapters)
        {
            if (chapter.Number != chapterNumber)
            {
                continue;
            }


            foreach (
                string attributeId
                in chapter.AttributeIds)
            {
                AttributeDefinitionData attribute =
                    attributeRepository.Load(
                        attributeId
                    );


                if (attribute != null)
                {
                    result.Add(
                        attribute
                    );
                }
            }


            break;
        }


        return result;
    }
}