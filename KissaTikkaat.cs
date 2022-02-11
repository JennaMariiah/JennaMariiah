using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class KissaTikkaat : PhysicsGame
{
    /// <summary>
    /// @Version 1
    /// @Author Jenna Hintsala
    /// </summary>

    IntMeter pisteLaskuri;

    private Image[]KissanKavely = LoadImages("animaatio01", "animaatio02", "animaatio03", "animaatio04");

    public override void Begin()
    {      
        ///Luodaan tausta
        Level.Background.CreateGradient(Color.White, Color.Blue);
        
        ///kutsutaan kentta
        LuoKentta();

        ///luodaan alareuna kenttaan
        Surface alaReuna = Surface.CreateBottom(Level);
        Add(alaReuna);

        /// luodaan kissa jota voidaan käyttää parametrina ja lisataan sille animaatio
        PhysicsObject kisu = LuoKissa(this, 0, 700);
        kisu.Animation = new Animation(KissanKavely);
        kisu.Animation.Start();

        /// asetetaan kamera seuraamaan pelaajaa
        Camera.FollowY(kisu);

        ///Zoomataan kamera jotta kentta tayttaa nayton
        Camera.Zoom(2.2);

        /// Kutsutaan lankakera aliohjelmaa
        LuoLankakera(this, 10, 10, 10);

        ///Luodaan pistelaskuri
        LuoPistelaskuri();

        ///Lisataan pisteita kun kissa osuu lankakeriin ja tuhotaan kerat ja portaat niihin osuttaessa
        AddCollisionHandler(kisu, "kera", CollisionHandler.AddMeterValue(pisteLaskuri, 1));
        AddCollisionHandler(kisu, "kera", Osui);
        AddCollisionHandler(kisu, "oikea", Osui);

        ///Lisataan taustamusiikki
        MediaPlayer.Play("kisumusa");

        ///Laitetaan taustamusiikki repeatille
        MediaPlayer.IsRepeating = true;

        ///Lisataan nappaimiston kuuntelijat 
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaKissaa, null, new Vector(-500, 0), kisu);
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaKissaa, null, new Vector(500, 0), kisu);
        Keyboard.Listen(Key.Up, ButtonState.Down, LiikutaKissaa, null, new Vector(0, 500), kisu);
        Keyboard.Listen(Key.Down, ButtonState.Down, LiikutaKissaa, null, new Vector(0, -500), kisu);
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Luodaan pelin kentta tekstitiedostosta
    /// </summary>
    private void LuoKentta()
    {
        TileMap ruudut = TileMap.FromLevelAsset("kentta");
        ruudut.SetTileMethod('M', LuoMaa);
        ruudut.SetTileMethod('N', LuoMaa);
        ruudut.SetTileMethod('P', LuoTikkaat);
        ruudut.SetTileMethod('V', LuoTikkaat);
        ruudut.SetTileMethod('S', LuoSeina);
        ruudut.Execute(20, 20);
    }


    /// <summary>
    /// Luodaan maapalkit kenttaan
    /// </summary>
    /// <param name="paikka">palkkien paikka</param>
    /// <param name="leveys">palkkien leveys</param>
    /// <param name="korkeus">palkkien korkeus</param>
    private void LuoMaa(Vector paikka, double leveys, double korkeus)
    {
            PhysicsObject maa = PhysicsObject.CreateStaticObject(leveys, korkeus);
            maa.Position = paikka;
	        maa.Color = Color.Harlequin;
	        maa.Tag = "rakenne";
	        maa.Image = LoadImage("ruoho01");
	        Add(maa);
    }


    /// <summary>
    /// Luodaan seinat kenttaan
    /// </summary>
    /// <param name="paikka">sijainti johon seina tulee</param>
    /// <param name="leveys">seinan leveys</param>
    /// <param name="korkeus">seinan korkeus</param>
    private void LuoSeina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maa = PhysicsObject.CreateStaticObject(leveys, korkeus);
        maa.Position = paikka;
        maa.Color = Color.Harlequin;
        maa.Tag = "rakenne";
        maa.Image = LoadImage("seina");
        Add(maa);
    }


    /// <summary>
    /// Luodaan tikkaat joita pitkin kissa paasee kiipeamaan
    /// </summary>
    /// <param name="paikka">tikkaiden paikka</param>
    /// <param name="leveys">tikkaiden leveys</param>
    /// <param name="korkeus">tikkaiden korkeus</param>
    /// <param name="vari">tikkaiden vari</param>
    /// <returns>tikkaat</returns>
    private void LuoTikkaat(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tikkaat = new PhysicsObject(leveys, korkeus * 1.8);
        tikkaat.Position = paikka;
        tikkaat.Color = Color.Brown;
        tikkaat.Tag = "oikea";
        tikkaat.Image = LoadImage("tikapuut");
        Add(tikkaat);

    }


    /// <summary>
    /// Luodaan pelaajan kissahahmo
    /// </summary>
    /// <param name="peli">peli johon kissa lisätään</param>
    /// <param name="x">kissan x-koordinaatti</param>
    /// <param name="y">kissan y-koordinaatti</param>
    /// <returns>palauttaa kisun jotta sitä voidaan käyttää muissa ohjelmissa</returns>
    private static PhysicsObject LuoKissa(Game peli, double x, double y)
    {
        PhysicsObject kissa = new PhysicsObject(30, 30);
        kissa.X = x;
        kissa.Y = y-1520;
        
        peli.Add(kissa, 3);
        kissa.CanRotate = false;
        return kissa;
        
    }


    /// <summary>
    /// Luodaan lankakerat joita kerataan
    /// </summary>
    /// <param name="peli">peli johon lankakerat lisataan</param>
    /// <param name="leveys">lankakeran leveys</param>
    /// <param name="pituus">lankakeran pituus</param>
    /// <param name="x">lankakeran x-koordinaatti</param>
    private static void LuoLankakera(Game peli, double leveys, double pituus, double x)
    {
        double a = peli.Level.Bottom;
        for (int i = 0; i < 28; i++)
        {
            
            PhysicsObject kera = new PhysicsObject(leveys, pituus);
            kera.X = x;
            kera.Y = a;
            kera.Image = LoadImage("lankakera");
            peli.Add(kera);
            a += 60;
            kera.Tag = "kera";
        }
        
    }


    /// <summary>
    /// Aliohjelma liikuttaa kissaa.
    /// </summary>
    /// <param name="vektori">Suunta, johon kissaa liikutetaan.</param>
    private void LiikutaKissaa(Vector vektori, PhysicsObject kissa)
    {
        kissa.Push(vektori);
    }


    /// <summary>
    /// Luodaan pistelaskuri
    /// </summary>
    private void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);
        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 100;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Score";
        IntMeter keratytEsineet = new IntMeter(0);
        pisteLaskuri.MaxValue = 28;
        pisteLaskuri.UpperLimit += KaikkiKeratty;
        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Luodaan ohjelma jolla tuhotaan kohde johon toramataan
    /// </summary>
    /// <param name="pelaaja">pelaaja joka tormaa</param>
    /// <param name="kohde">kohde joka tuhotaan</param>
    private void Osui(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        kohde.Destroy();
    }


    /// <summary>
    /// Naytetaan teksti kun pisteet on taynna
    /// </summary>
    private void KaikkiKeratty()
    {
        MessageDisplay.Add("Kissa pääsi takaisin taivaaseen. Voitit pelin!");
    }


}
