namespace backend.kapace.DAL.Models;

public record Translator(long Id, string Name, string Link) {
    public static Translator CreateInsertModel(string Name, string Link) {
        return new Translator(0, Name, Link);
    }
};