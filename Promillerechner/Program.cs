namespace Promillerechner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PromilleRechner promilleRechner = new PromilleRechner();
            Person person = promilleRechner.Person;
            Getraenk getraenk = promilleRechner.Getraenk;
            person.Trinke(getraenk);
            Spruch spruch = new Spruch(person);
            Console.WriteLine(spruch.GetSpruch());
        }
    }
}
#region Person
public class Person
{
    public const double AnteilWasserimBlut = 0.8;
    public const double DichteBlut = 1.055;
    public const int MAENNLICH = 0;
    public const int WEIBLICH = 1;
    public const double ABBAU_WARTEZEIT_STUNDEN = 1.0;
    public const double ABBAU_PRO_STUNDE = 0.1;

    private double _koerpermasse;
    private double _koerpergroesse;
    private int _alter;
    private int _geschlecht;
    private double _alkoholPromille;

    public Person(double koerpermasse, double koerpergroesseInCm, DateTime geburtsdatum, int geschlecht)
    {
        _koerpermasse = koerpermasse;
        _koerpergroesse = koerpergroesseInCm;
        _alter = AlterInJahren(geburtsdatum);
        _geschlecht = geschlecht;
    }
    private int AlterInJahren(DateTime geburtsdatum)
    {
        double alter =  (DateTime.Now - geburtsdatum).TotalDays / 365; // why do we use use (DateTime.Now - geburtsdatum).TotalDays / 365? why not just ask them how old there are?
        return (int)alter;
    }
    public double GWK
    {
        get
        {
            if (_geschlecht == MAENNLICH)
            {
                return 2.447 - 0.09516 * _alter + 0.1074 * _koerpergroesse + 0.3362 * _koerpermasse;
            }
            else if (_geschlecht == WEIBLICH)
            {
                return 0.203 - 0.07 * _alter + 0.1069 * _koerpergroesse + 0.2466 * _koerpermasse;
            }
            else
            {
                throw new InvalidOperationException("Geschlecht muss entweder MAENNLICH oder WEIBLICH sein.");
            }
        }
    }
    public void Trinke(Getraenk getraenk)
    {
        double promille = (getraenk.AlkoholmasseInGramm * Person.AnteilWasserimBlut) / (GWK * Person.DichteBlut); 
        _alkoholPromille += AlkoholAbbau(getraenk, promille);
    }
    public double AlkoholPromille
    {
        get { return _alkoholPromille; }
        set { _alkoholPromille = value; }
    }
    private double AlkoholAbbau(Getraenk getraenk, double promille)
    {
        return Math.Max(promille - (getraenk.StundenSeitAbbau * ABBAU_PRO_STUNDE), 0);
    }
}
#endregion

#region Getraenk
public class Getraenk
{
    public const double DICHTE_ALKOHOL = 0.8;

    protected double _alkoholgehalt;
    protected double _volumenInMilliliter;
    protected DateTime _getrunkenAm;
    protected double _stundenSeitEinnahme;

    public Getraenk(double volumenInMilliliter, double alkoholgehalt, DateTime getrunkenAm)
    {
        _volumenInMilliliter = volumenInMilliliter;
        _alkoholgehalt = alkoholgehalt;
        _getrunkenAm = getrunkenAm;
        _stundenSeitEinnahme = (DateTime.Now - getrunkenAm).TotalHours;
    }

    public virtual double AlkoholmasseInGramm
    {
        get
        {
            return _volumenInMilliliter * _alkoholgehalt * DICHTE_ALKOHOL;
        }
    }
    public double StundenSeitAbbau
    {
        get {
            return Math.Max(_stundenSeitEinnahme - 1, 0);
        }
    }
}
public class Bier : Getraenk
{
    public const double ALKOHOLGEHALT = 0.05;

    public Bier(double volumenInMilliliter, DateTime getrunkenAm)
        : base(volumenInMilliliter, ALKOHOLGEHALT, getrunkenAm)
    {

    }
}
public class Wein : Getraenk
{
    public const double ALKOHOLGEHALT = 0.10;

    public Wein(double volumenInMilliliter, DateTime getrunkenAm)
        : base(volumenInMilliliter, ALKOHOLGEHALT, getrunkenAm)
    {

    }
}
public class Schnaps : Getraenk
{
    public const double ALKOHOLGEHALT = 0.40;

    public Schnaps(double volumenInMilliliter, DateTime getrunkenAm)
        : base(volumenInMilliliter, ALKOHOLGEHALT, getrunkenAm)
    {

    }
}
#endregion

#region PromilleRechner
public class PromilleRechner
{
    private Person _person;

    public Person Person
    {
        get
        {
            if (_person == null)
            {
                _person = FragePersonenDaten();
            }
            return _person;
        }
    }

    private Getraenk _getraenk;

    public Getraenk Getraenk
    {
        get
        {
            if (_getraenk == null)
            {
                string getraenkeTyp = StringInput("Was haben Sie Getrunken? Bier, Wein oder Schnaps: ");
                double volumenInMilliliter = StringInputToDouble("Geben Sie das Volumen des Getränks in ml ein: ");
                DateTime getrunkenAm = StringInputToDateTime("Geben Sie das Datum und die Uhrzeit des Getränks ein (z.B. 01.01.2000 20:00): ");
                _getraenk = FrageGetraenkeDaten(volumenInMilliliter, getraenkeTyp, getrunkenAm);
            }
            return _getraenk;
        }
    }

    public Person FragePersonenDaten()
    {
        double koerpermasse = StringInputToDouble("Geben Sie die Körpermasse in kg ein(z.b 80): ");
        double koerpergroesseInCm = StringInputToDouble("Geben Sie die Körpergröße in cm ein(z.b 180): ");
        DateTime geburtsdatum = StringInputToDateTime("Geben Sie das Geburtsdatum ein (z.B. 01.01.2000): ");
        int geschlecht = StringInputToInt("Geben Sie das Geschlecht ein (0 für männlich, 1 für weiblich): ");
        return new Person(koerpermasse, koerpergroesseInCm, geburtsdatum, geschlecht);
    }

    public Getraenk FrageGetraenkeDaten(double volumenInMilliliter, string getraenkeTyp, DateTime getrunkenAm)
    {
        // Check the drink type and create the corresponding drink
        switch (getraenkeTyp.ToLower())
        {
            case "bier":
                return new Bier(volumenInMilliliter, getrunkenAm);
            case "wein":
                return new Wein(volumenInMilliliter, getrunkenAm);
            case "schnaps":
                return new Schnaps(volumenInMilliliter, getrunkenAm);
            default:
                throw new ArgumentException("Unbekannter Getränketyp.");
        }
    }
    protected string StringInput(string question)
    {
        Console.Write(question);
        return Console.ReadLine();
    }
    protected double StringInputToDouble(string question)
    {
        double result;
        Console.Write(question);
        if (!double.TryParse(Console.ReadLine(), out result))
        {
            throw new ArgumentException("Ungültige Eingabe.");
        }
        return result;
    }

    protected DateTime StringInputToDateTime(string question)
    {
        DateTime result;
        Console.Write(question);
        if (!DateTime.TryParse(Console.ReadLine(), out result))
        {
            throw new ArgumentException("Ungültige Eingabe.");
        }
        return result;
    }

    protected int StringInputToInt(string question)
    {
        int result;
        Console.Write(question);
        if (!int.TryParse(Console.ReadLine(), out result))
        {
            throw new ArgumentException("Ungültige Eingabe.");
        }
        else if(result != Person.MAENNLICH && result != Person.WEIBLICH)
        {
            throw new ArgumentException("Geschlecht muss entweder MAENNLICH oder WEIBLICH sein.");
        }   
        return result;
    }
}
#endregion

#region Spruch
public class Spruch
{
    public Spruch(Person Person)
    {
        _AlkoholPromille = Person.AlkoholPromille;
    }
    private double _AlkoholPromille;
    public string GetSpruch()
    {
        
        switch (_AlkoholPromille)
        {
            case < 0.0:
                return "Du bist nüchtern!";
            case < 0.3:
                return "Du darfst noch Autofahren!";
            case < 1.0:
                return "Du bist angetrunken!";
            case < 2.0:
                return "Du bist betrunken!";
            case < 3.0:
                return "Du bist stark betrunken!";
            case < 4.0:
                return "Du bist sehr stark betrunken!";
            default:
                return "Du solltest dringend aufhören zu trinken!";
        }
    }
}
#endregion