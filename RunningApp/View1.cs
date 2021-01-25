using Android.Graphics;
using System;
using System.Collections.Generic;
using Android.Hardware; 
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Kaart;
using Android.Locations;
using Android.Util;
using Android.App;


namespace RunningApp
{
    public class RunningView1 : View, ScaleGestureDetector.IOnScaleGestureListener, GestureDetector.IOnGestureListener, ILocationListener, ISensorEventListener
    {
        Bitmap Plaatje, Marker;
        float schaal, midx, midy, Hoek;
        ScaleGestureDetector detector;
        GestureDetector detector2;

        bool fake;

        bool bijhouden;                                                     // De bool is nodig voor het activeren van het bijhouden van de track door op start te drukken.
        public static List<PointF> Track = new List<PointF>();              // Houdt de punten bij die worden gegenereerd tijdens de track
        public static List<PointF> FakeTrack = new List<PointF>();          // Houdt de punten bij van de fake-track
        public static List<LocatieTijd> Loctijden = new List<LocatieTijd>();



        //bool analyze;

        PointF centrum = new PointF(138300, 454300);        //positie van het midden van de kaart 
        PointF huidig = new PointF(138300, 454400);         //galgenwaard

        Matrix mat;

        Context context;

        string locatiebericht;

        public RunningView1(Context c) : base(c)
        {
            this.SetBackgroundColor(Color.AntiqueWhite);
            this.Touch += RaakAan;

            context = c;

            fake = true;

            // De bitmaps voor de kaart en de location-marker
            BitmapFactory.Options opt = new BitmapFactory.Options();
             opt.InScaled = false;
            Plaatje = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.kaart, opt);
            Marker = BitmapFactory.DecodeResource(c.Resources, Resource.Drawable.gpsarrow, opt) ;

            // Sensor voor de kompasrichting van de location-marker
            SensorManager sm = (SensorManager)c.GetSystemService(Context.SensorService);
            sm.RegisterListener(this, sm.GetDefaultSensor(SensorType.Orientation), SensorDelay.Ui);

            // De constructor-methode waar de variabele detector zijn waarde krijgt
            detector = new ScaleGestureDetector(c, this);
            detector2 = new GestureDetector(c, this);

            // Beginwaarde van de schaal die wordt meegegeven
            schaal = 1f;

            // De registratie van de listener voor de location
            LocationManager lm = (LocationManager)c.GetSystemService(Context.LocationService);
            Criteria crit = new Criteria();
            crit.Accuracy = Accuracy.Fine;
            //crit.SpeedRequired = true;
            //crit.SpeedAccuracy = Accuracy.High;
            string lp = lm.GetBestProvider(crit, true);
            if (lp != null)
                lm.RequestLocationUpdates(lp, 1000, 4, this);
               

        }


        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            //if (schaal == 0)
            //schaal = Math.Max(((float)this.Width) / this.Plaatje.Width, ((float)this.Height) / this.Plaatje.Height);

            // Definities voor Paint
            Paint verf = new Paint();
            Paint verfrood = new Paint();
            verfrood.Color = Color.Red;

            // Het berekenen van de afstand in pixels
            midx =  (centrum.X - 136000) * 0.4f;
            midy = -(centrum.Y - 458000) * 0.4f;

            // Tekenen van de kaart
            mat = new Matrix();
            mat.PostTranslate(-midx, -midy );
            mat.PostScale(this.schaal, this.schaal);
            mat.PostTranslate(Width / 2f, Height / 2f);
            canvas.DrawBitmap(this.Plaatje, mat, verf);

            //// Tekenen van de location-marker
            mat = new Matrix();
            mat.PostTranslate(-Marker.Width / 2, -Marker.Height / 2);
            mat.PostRotate(this.Hoek);


            // Hier gebruiken we 0.07f omdat de afbeelding erg groot is en verkleind moet worden, daaarnaast wordt er een extra verkleining uitgevoerd afhankelijk van de schaal
            mat.PostScale(0.07f - (0.003f * this.schaal), 0.07f - (0.003f * this.schaal));
            mat.PostTranslate(this.Width / 2 + (huidig.X - centrum.X) * 0.4f * this.schaal, this.Height / 2 + (centrum.Y - huidig.Y) * 0.4f * this.schaal);
            canvas.DrawBitmap(this.Marker, mat, verf);


            // Tekenen van de bijgehouden track
            // De bool 'fake' is gelijk aan false wanneer de gebruiker op start heeft gedrukt

            if (fake == false)
            {
                foreach (LocatieTijd loctijd in Loctijden)
                {
                    PointF p = loctijd.Locatie;
                    DateTime t = loctijd.Tijd;

                    float ax = p.X - centrum.X;
                    float px = ax * 0.4f;
                    float sx = px * schaal;
                    float x = this.Width / 2 + sx;

                    float ay = -p.Y + centrum.Y;
                    float py = ay * 0.4f;
                    float sy = py * schaal;
                    float y = this.Height / 2 + sy;
                    canvas.DrawCircle(x, y, 20, verfrood);

                }
            }

            // De bool 'fake' is true wanneer de app is gestart en de gebruiker nog niet op start heeft gedrukt
            // Er wordt dan een fake-track getekend op de kaart

            if (fake == true)
            {
                FakeTrack.Add(new PointF(138300, 454400));
                FakeTrack.Add(new PointF(136876, 455925));

                new DateTime()

                foreach (PointF p in FakeTrack)
                {
                    float ax = p.X - centrum.X;
                    float px = ax * 0.4f;
                    float sx = px * schaal;
                    float x = this.Width / 2 + sx;

                    float ay = -p.Y + centrum.Y;
                    float py = ay * 0.4f;
                    float sy = py * schaal;
                    float y = this.Height / 2 + sy;
                    canvas.DrawCircle(x, y, 20, verfrood);

                }
            }
        }

        // De event-handler voor de drag en pinch gesture
        public void RaakAan(object o, TouchEventArgs tea)
        {
            detector.OnTouchEvent(tea.Event);
            detector2.OnTouchEvent(tea.Event);

            this.Invalidate();
        }


        // Hieronder staan de event-handlers van de knoppen Centreer, Start, Verwijder :
        public void Centreer(object o, EventArgs ea)
        {
            this.centrum = this.huidig;
            this.Invalidate();
        }

        public void Start(object o, EventArgs ea)
        {
            this.bijhouden = true;

            fake = false; 

            this.Invalidate();

        }

        public void Verwijder(object o, EventArgs ea)
        {
            Track.Clear();

            FakeTrack.Clear();

            fake = false;

            this.Invalidate();
           
        }

        public void Deel(object o, EventArgs ea)
        {
            locatiebericht = "";
            foreach (LocatieTijd loctijd in Loctijden)
            {
                PointF p = loctijd.Locatie;
                DateTime t = loctijd.Tijd;
                locatiebericht += p.X + "-" + p.Y + "\n" + t.ToString() + "\n" + "\n";
            }
            string bericht = "Kijk mijn locaties: " + locatiebericht;
            Intent i = new Intent(Intent.ActionSend);
            i.SetType("text/plain");
            i.PutExtra(Intent.ExtraText, bericht);
            context.StartActivity(i);
        }

       

        //public void TijdBijhouden()
        //{
        //    foreach (PointF p in Track)
        //    {
        //        Track.Add(huidig)
               
        //    }

        //    this.Invalidate();

        //}


        // Event-handler voor het analyseren van routes

        public void Analyze(object o, EventArgs ea)
        {
            Intent i = new Intent(context, typeof(AnalyzeAct));

            context.StartActivity(i);
        }


        // Methodes voor PINCH
        public bool OnScale(ScaleGestureDetector detector)
        {
            this.schaal *= detector.ScaleFactor;

            // Dit geeft de minimale en maximale schaal aan voor het pinchen.
            this.schaal = (float)Math.Max(((float)this.Height / this.Plaatje.Height), Math.Min(schaal, 5));


            this.Invalidate();
            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
        
        }


        // Methodes voor DRAG
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            float dragX = (distanceX / schaal)  / 0.4f;
            float dragY = (distanceY / schaal)  / 0.4f;

            // Borders zodat de kaart niet uit het scherm kan gaan.
            centrum.X = Math.Max(centrum.X, 136000); 
            centrum.X = Math.Min(centrum.X, 142000); 
            centrum.Y = Math.Min(centrum.Y, 458000); 
            centrum.Y = Math.Max(centrum.Y, 453000); 

            this.centrum = new PointF(centrum.X + dragX, centrum.Y - dragY);

            this.Invalidate();
            return true;
        }

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {

        }

        public void OnShowPress(MotionEvent e)
        {

        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        //Methodes voor de Locatie
        public void OnLocationChanged(Location loc)
        {
            huidig = Projectie.Geo2RD(loc);
            if (bijhouden)
            {
                Track.Add(huidig);
                Loctijden.Add(new LocatieTijd(huidig, DateTime.Now));

            }

            this.Invalidate();
        }


        public void OnProviderDisabled(string provider)
        {
          
        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }


        //Methodes voor Kompasrichting
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            this.Hoek = e.Values[0];
            this.Invalidate();
        }
    }
}



