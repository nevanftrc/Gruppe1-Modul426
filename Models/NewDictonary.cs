using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyWordWPF_US5.Models
{
    /// <summary>
    /// Enthaltet die wörter
    /// </summary>
    public class NewDictonary
    {
        /// <summary>
        /// Gibt die wörter zurück damit es nicht redudant wird
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetWords() 
        {
                HashSet<string> words = new HashSet<string>{"eins", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun", "zehn", "elf",
                "hundert", "tausend", "Million", "Milliarde", "erste", "zweite", "dritte",
                "Montag", "Dienstag", "Samstag", "Daumen", "Mund", "Angestellte", "Sontag", "Donnerstag", "Freitag", "Juni",
                "Po", "eine", "wo","wieveil", "wie", "was", "warum", "wann","Bewerbung", "Beruf", "Arbeit", "Ausbildung", "Ohr",
                "Haar", "Gesicht", "Auge", "Nase", "Gewebe", "Kinn", "Wange", "Stirn", "Hals","Nacken","Brust","Bauch","Bein", "Arm",
                "Ellenbogen", "Fingernagel", "Kehle", "Lippe","Trommelfell","Knie","Rippe","Lunge","Leber","Blut","Darm","Niere",
                "Muskel","Skelett","Haut","Zunge","Knochen","Sehne", "Januar", "Februar", "April", "Mai", "Juli", "August", "September", "Oktober",
                "November","Dezember", "Lehrstelle" };
            return words;
        }
    }
}
