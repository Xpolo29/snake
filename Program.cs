using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

public class Program
{
    public static void Main()
    {
        //###########################PART_1 BEGIN##############################
        //##### Application initialisation ######



        //Var init
        //Pathfinding algo value

        int dij_radius = 0; //The number of cases around snake to color, debugging
        int see_length = 40; //Range of auto-dodge raycast
        float heta = 10f; //Tendency to stick to his body

        //script var begin
        int filling=0;
        int start_score = 1;
        List<Object> obj_l = new List<object>();
        int max_score = 0;
        int score = 1;
        int gene = 0;
        bool blocked = false;
        Pen crayon = new Pen(Color.Green);
        bool dead = false;
        int current_fps = 1;
        Point apple_pos = new Point(0, 0);
        Size MainSize = new Size(768, 512);
        System.Windows.Forms.Timer fps = new System.Windows.Forms.Timer();
        fps.Interval = 1000 / current_fps;
        SolidBrush crayong = new SolidBrush(Color.Green);
        SolidBrush crayonw = new SolidBrush(Color.WhiteSmoke);
        SolidBrush crayonb = new SolidBrush(Color.Blue);
        SolidBrush crayonc = new SolidBrush(Color.DarkCyan);
        SolidBrush crayonr = new SolidBrush(Color.Red);
        SolidBrush crayongray = new SolidBrush(Color.LightGray);
        Point head_pos = new Point(0, 0);
        Point last_d = head_pos;
        Point last_pos = head_pos;
        List<Point> bodyparts = new List<Point>();
        List<Point> direction = new List<Point>();
        List<Point> tracker = new List<Point>();
        direction.Add(new Point(10, 0));
        direction.Add(new Point(0, 10));
        direction.Add(new Point(-10, 0));
        direction.Add(new Point(0, -10));
        Random random = new Random();
        float alpha = random.Next(5, 100) / 100f;
        float beta = random.Next(5, 100) / 100f;
        float epsilon = 0.25f;
        float[,] Q_table = new float[11, 4];
        int last_choice = 0;
        int[,,] map = new int[73, 36, 2];
        //for (int i = 0; i < 11; i++)
        //{
        //    for (int j = 0; j < 4; j++)
        //    {Q_table[i, j] = 0;}
        //}

        //win init
        Form win = new Form()
        {
            Text = "LaboH",
            Size = MainSize,
            MinimumSize = MainSize,
            MaximumSize = MainSize,
            BackColor = Color.Black,
        };


        //Grid
        Panel grid = new Panel()
        {
            Parent = win,
            Location = new Point(16, 10),
            Size = new Size(MainSize.Width - 40, MainSize.Height - 150),
            BackColor = Color.WhiteSmoke

        };


        //Bot Panel
        Panel bot_panel = new Panel()
        {
            Parent = win,
            Size = new Size(MainSize.Width - 10, 100),
            Dock = DockStyle.Bottom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };


        //Fps gauge
        TrackBar gauge = new TrackBar()
        {
            Parent = bot_panel,
            Maximum = 200,
            Minimum = 1,
            Size = new Size(200, 30),
            Location = new Point(MainSize.Width - 220, 50),
            BackColor = Color.LightGray,
            TickStyle = TickStyle.None,
        };

        //Score counter
        Label counter = new Label()
        {

            Parent = bot_panel,
            Size = new Size(80, 25),
            Location = new Point(MainSize.Width - 260, 5),
            Text = "Size = " + score.ToString(),
            ForeColor = Color.Black
        };

        //Alpha beta monitor
        Label alphbeta = new Label()
        {

            Parent = bot_panel,
            Size = new Size(200, 70),
            Location = new Point(MainSize.Width - 650, 5),
            ForeColor = Color.Black,
        };

        //Max score counter
        Label max_counter = new Label()
        {

            Parent = bot_panel,
            Size = new Size(100, 25),
            Location = new Point(MainSize.Width - 360, 5),
            Text = "Max = " + max_score.ToString(),
            ForeColor = Color.Black
        };


        //Gen counter
        Label gen = new Label()
        {

            Parent = bot_panel,
            Size = new Size(70, 25),
            Location = new Point(MainSize.Width - 450, 5),
            Text = "Gen = " + gene.ToString(),
            ForeColor = Color.Black
        };

        //Fps Counter
        Label fps_counter = new Label()
        {

            Parent = bot_panel,
            Size = new Size(70, 25),
            Location = new Point(MainSize.Width - 140, 5),
            ForeColor = Color.Black
        };


        Label debug = new Label()
        {

            Parent = bot_panel,
            Size = new Size(200, 80),
            Location = new Point(MainSize.Width - 700, 5),
            ForeColor = Color.Black
        };

 
        //State
        Label state = new Label()
        {

            Parent = bot_panel,
            Size = new Size(70, 25),
            Location = new Point(MainSize.Width - 768, 80),
            ForeColor = Color.Black

        };


        //Start Button
        Button start_b = new Button()
        {
            Text = "Start the simulation ",
            Size = new Size(70, 20),
            Parent = bot_panel,
            ForeColor = Color.Black
        };



        //Late var init
        Graphics g = grid.CreateGraphics();
        start_b.SetBounds(320, 70, 150, 25);
        win.FormClosed += stop;
        win.Activated += focused;
        gauge.ValueChanged += fps_calc;
        Rectangle Border = new Rectangle(0, 0, grid.Size.Width - 1, grid.Size.Height - 1);
        start_b.Click += start_clicked;
        win.ClientSizeChanged += resize_all;
        fps.Tick += update;
        gauge.Value = 100;
        //Finally start app
        Application.Run((win));
        //###############################PART1_END#############################














        //###############################PART2_BEGIN###########################
        //######## Snake Game ########

        //Check if point is in bounds
        bool inborder(Point p)
        {
            if ((p.X >= 724) || (p.X < 0)
            || (p.Y >= 358) || (p.Y < 0))
            { return false; }
            return true;
        }

        //reset map to 0s
        void clearmap()
        {
            for (int i = 0; i < 73; i++)
            {
                for (int j = 0; j < 36; j++)
                {
                    map[i, j, 0] = 0;
                    map[i, j, 1] = 0;
                }
            }
        }

        //Print map
        void printmap(int layer)
        {
            for (int i = 0; i < 73; i++)
            {
                for (int j = 0; j < 36; j++)
                {
                    Console.WriteLine(map[i, j, layer]);
                }
            }
        }


        //Update fonction
        void update(Object obj, EventArgs e)
        {
            //BEGIN update
            g.FillRectangle(crayonr, apple_pos.X, apple_pos.Y, 9, 9);

            //update data print
            alphbeta.Text = "Head pos = " + new Point(head_pos.X/10, head_pos.Y/10) + "; \n" +
            "Dpomme= " + d_points(head_pos, apple_pos)/10+"; ";

            if (max_score <= score)
            { max_score = score; max_counter.Text = "Max = " + max_score.ToString(); }

            //call AI
            int v1 = AI_1();

            string di = "";
            if (v1 == 0) { di = "right"; }
            if (v1 == 1) { di = "down"; }
            if (v1 == 2) { di = "left"; }
            if (v1 == 3) { di = "up"; }
            alphbeta.Text += "go " + di + "; ";

            //Move
            Point temp = direction[v1];
            Point new_pos = new Point(temp.X + head_pos.X, temp.Y + head_pos.Y);
            if (!new_pos.Equals(last_pos) || (score == 1))
            {
                last_d = temp;
                last_pos = head_pos;
                head_pos = new_pos;
                if (blocked)
                { blocked = false; }
                if ((bodyparts.Contains(head_pos) && (!head_pos.Equals(last_pos)))
                  || !inborder(head_pos))
                {
                    Console.WriteLine("DEAD !");
                    fps.Stop();
                    state.Text = "Dead";
                    alphbeta.Text += "Dead; ";
                    dead = true;
                    state.BackColor = Color.Red;

                }
                DrawPoint(head_pos);
            }
            else
            {
                debug.Text += "Wrong Direction !; ";
                Console.WriteLine("Wrong Direction !");
                blocked = true;
            }



            if (!bodyparts.Contains(head_pos))
            { bodyparts.Add(head_pos); }
            apple(head_pos);
            if (dead) { start_clicked(new Object(), new EventArgs()); }
            //END update
        }


        //Start && restart button
        void start_clicked(Object obj, EventArgs e)
        {
            start_b.Text = "Restart";
            clean_tracker();
            head_pos = rand_p();
            g.Clear(Color.WhiteSmoke);
            g.DrawRectangle(crayon, Border);
            //Console.WriteLine(Border.Width.ToString()+" by "+ Border.Height.ToString()+" anchored at " +Border.Location.ToString());
            bodyparts.Clear();
            apple_pos = rand_p();
            g.FillRectangle(crayonr, apple_pos.X, apple_pos.Y, 9, 9);
            state.Text = "Alive";
            state.BackColor = Color.LightGreen;
            score = start_score;
            counter.Text = "Size = " + score.ToString();
            dead = false;
            gene++;
            gen.Text = "Gen = " + gene.ToString();
            filling = 0;
            fps.Start();
        }


        //Stop App fonction
        void stop(Object obj, EventArgs e)
        {
            fps.Stop();
        }


        //Restored focus fonction
        void focused(Object obj, EventArgs e)
        {
            //Console.WriteLine("Restored Focus ! Drawing "+(bodyparts.Count +1).ToString() + " squares.");

            foreach (Point p in bodyparts)
            {
                g.FillRectangle(crayong, p.X, p.Y, 9, 9);
            }
        }


        //Update fps fonction
        void fps_calc(Object obj, EventArgs e)
        {
            current_fps = gauge.Value;
            fps.Interval = 1000 / current_fps;
            fps_counter.Text = current_fps.ToString() + " / s";
        }


        //Resize fonction (deprecated)
        void resize_all(Object obj, EventArgs e)
        {
            Point op = new Point(bot_panel.Size.Width, bot_panel.Size.Height);
            counter.Location = new Point(Convert.ToInt32(op.X * 0.95), 0);
            start_b.SetBounds(Convert.ToInt32(op.X * 0.5 - start_b.Size.Width * 0.5),
               Convert.ToInt32(op.Y * 0.70), 150, 25);
            //Console.WriteLine("Resizing...");
        }


        //Draw Head and erase tail fonction
        void DrawPoint(Point head)
        {
            g.FillRectangle(crayong, head.X, head.Y, 9, 9);
            if (bodyparts.Count >= score)
            {
                Point tail = bodyparts[0];
                bodyparts.Remove(tail);
                g.FillRectangle(crayonw, tail.X, tail.Y, 9, 9);
            }
        }


        //Apple fonction
        void apple(Point head)
        {
            Rectangle apple_rect = new Rectangle(apple_pos.X, apple_pos.Y, 9, 9);
            Rectangle head_rect = new Rectangle(head.X, head.Y, 9, 9);
            if (head_rect.IntersectsWith(apple_rect))
            {
                score++;
                g.FillRectangle(crayonw, apple_rect);
                g.FillRectangle(crayong, head_rect);
                apple_pos = rand_p();
                apple_rect.Location = apple_pos;
                g.FillRectangle(crayonr, apple_rect);
                counter.Text = "Size = " + score.ToString();
                clean_tracker();
                clearmap();
                
            }
        }

        //Generate a random point on grid
        Point rand_p()
        {
            Point p = new Point(random.Next(1, 72), random.Next(1, 35));
            p = new Point(p.X * 10, p.Y * 10);
            if (bodyparts.Contains(p))
            { return rand_p(); }
            return p;
        }

        int d_points(Point a, Point b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            double d = Math.Sqrt(x*x+ y*y) - 1.0;

            return Convert.ToInt32(d);
        }


        //###############################PART2_END#############################














        //###############################PART3_BEGIN###########################
        //_______Artificial Intelligence Part________
        //IA num 1
        //Direct PathFinding
        int AI_1()
        {
            debug.Text = "";
            //gauge.Value = 100;
            int vector1 = -1;
            /*
            Point px = new Point(0, 0);
            Point py = new Point(0, 0);
            int x = head_pos.X - apple_pos.X;
            int y = head_pos.Y - apple_pos.Y;
            int Abx = Math.Abs(x);
            int Aby = Math.Abs(y);
            int sx = Math.Sign(x);
            int sy = Math.Sign(y);
            clean_tracker();


            if (Abx > 0)
            {
                if (sx > 0) { vector1 = 2; }
                else { vector1 = 0; }
            }
            if (Aby > 0)
            {
                if (sy > 0) { vector1 = 3; }
                else { vector1 = 1; }
            }

            //Check if trajectoire is barred
            //x
            int k = bodyparts.Count / 20;
            for (int i = 10; i < Abx; i += 10)
            {
                px = new Point(i * -sx + head_pos.X, apple_pos.Y);
                tracker.Add(px);
            }
            //y
            for (int j = 10; j < Aby + 10; j += 10)
            {
                py = new Point(head_pos.X, j * -sy + head_pos.Y);
                tracker.Add(py);
            }
            //Console.WriteLine("-------OFF---------");
            */
            /*//vector1 = dodge3();
            if (map[head_pos.X / 10, head_pos.Y / 10, 1] - 1 <= 0)
            { //vector1 = check(vector1); }
                vector1 = dodge3();
            }
            else
            {
                debug.Text += "Path followed; ";
                vector1 = map[head_pos.X / 10, head_pos.Y / 10, 1] - 1;
            }*/
            vector1 = dodge3();
            int n = bodyparts.Count;
            int fuarea = free_area(head_pos, vector1, false,2*n,null);
            if (filling <= 0)
            {
                List<Point> L = new List<Point>();

                Point fpos = new Point(head_pos.X + direction[vector1].X, head_pos.Y + direction[vector1].Y);
                Point fpos2 = new Point(head_pos.X + 2*direction[vector1].X, head_pos.Y + 2*direction[vector1].Y);
                int frarea1 = free_area(fpos, vector1, false, 200 + n, null);
                if (inborder(fpos)) { L.Add(fpos); }
                int frarea2 = free_area(fpos, vector1, false, 200 + n, L);
                if (inborder(fpos2)) { L.Add(fpos2);}
                

                //Check in front
                if ((frarea1 != 0) && ((frarea2 != 0) && (Math.Abs(frarea1 - frarea2) > 10)))
                {
                    vector1 = dodge4(vector1);
                }

                int rarea = free_area(fpos, (vector1 + 1) % 4, false, 200 + n,L);
                int larea = free_area(fpos, (vector1 + 3) % 4, false, 200 + n,L);
                
                //Check sideways
                if ((larea != 0) && (rarea != 0) && (Math.Abs(rarea - larea) > 10))
                {
                    //Console.WriteLine("Trying not to close");
                    if (rarea - larea <= 0)
                    { vector1 = (vector1 + 3) % 4; filling = 5+n/80; }
                    else
                    { vector1 = (vector1 + 1) % 4;filling = 5+n/80; }
                   
                }


                //Check if area is enough for whole body
                fuarea = free_area(head_pos, vector1, false, 2 * n,null);

                if ((fuarea < n))
                {
                    alphbeta.Text += "\n Safety First; ";
                    vector1 = dodge4(-1);
                }

            }
            else
            {
                vector1 = dodge4(-1);
                filling--;
            }

            //Kill on stuck
            if (blocked) { vector1 = AI_2(); }
            //draw_tracker();
            last_choice = vector1;
            return vector1;
        }

        int dodge4(int not_this_dir)
        {
            int max = 0;
            int c = 0;
            int d2 = -1;
            for(int k=0;k<4;k++)
            {
                if ((k != (last_choice + 2) % 4) && (k!= not_this_dir))
                {
                    c = free_area(head_pos, k,false,9999,null);
                    if((head_pos.X+direction[k].X > 710) || (head_pos.X + direction[k].X < 11))
                    { c = c * 4 / 5; }
                    if ((head_pos.Y + direction[k].Y > 345) || (head_pos.Y + direction[k].Y < 11))
                    { c = c * 4 / 5; }
                    if ((c > max))
                    {
                        max = c;
                        d2 = k;
                    }
                }
            }
            if (d2 >= 0) { return d2; }
            return dodge2(last_choice,head_pos);
        }

        int free_area(Point P,int dir2,bool verbose,int max, List<Point> L)
        {
            clearmap();
            int A = 0;
            int step = 0;
            List<Point> queue = new List<Point>();
            List<Point> queue2 = new List<Point>();

            foreach (Point p in bodyparts)
            { map[p.X / 10, p.Y / 10, 0] = 999; }
            if (L != null)
            {
                foreach (Point p in L)
                { map[p.X / 10, p.Y / 10, 0] = 999; }
            }

            if (dir2 >= 0)
            {
                Point temp = new Point((P.X + direction[dir2].X), (P.Y + direction[dir2].Y));
                if ((inborder(temp) && (map[temp.X / 10, temp.Y / 10, 0] == 0)))
                {
                    map[(P.X + direction[dir2].X) / 10, (P.Y + direction[dir2].Y) / 10, 0] = 999;
                }
                else
                { return 0; }
                queue.Add(new Point((P.X + direction[dir2].X) / 10, (P.Y + direction[dir2].Y) / 10));

            }
            else
            { 
                if(inborder(P))
                {queue.Add(new Point(P.X / 10, P.Y / 10));}
            }




            while (queue.Count > 0)
            {

                for (int k = 0; k < queue.Count; k++)
                {
                    Point cpos = queue[k];
                    //Dans chaque direction
                    foreach (Point dir in direction)
                    {

                        Point npos = new Point(dir.X / 10 + cpos.X, dir.Y / 10 + cpos.Y);
                        Point tnpos = new Point(npos.X * 10, npos.Y * 10);

                        //If new pos is valid
                        if ((inborder(tnpos)) && map[npos.X, npos.Y, 0] == 0)
                        {
                            queue2.Add(npos);
                            A++;
                            map[npos.X, npos.Y, 0] = 1;
                        }
                    }
                }
                queue.Clear();
                foreach (Point p in queue2)
                { queue.Add(p); }
                queue2.Clear();
                step++;
                if (A > max) { return max; }
                if (step > 3000) { alphbeta.Text += "Area error; "; break; }
            }
            queue.Clear();
            queue2.Clear();

            if (verbose) { Console.WriteLine("Area is " + A); }
            return A;
        }

        //check if vector1 choice is good or not
        int check(int go)
        {
            Point p;
            int k2 = 0;
            p = new Point(head_pos.X - 10*2, head_pos.Y);
            if (bodyparts.Contains(p)) { k2++; }
            p = new Point(head_pos.X + 10*2, head_pos.Y);
            if (bodyparts.Contains(p)) { k2++; }
            p = new Point(head_pos.X, head_pos.Y - 10*2);
            if (bodyparts.Contains(p)) { k2++; }
            p = new Point(head_pos.X, head_pos.Y + 10*2);
            if (bodyparts.Contains(p)) { k2++; }

            int k1 = 0;
            p = new Point(head_pos.X - 10 * 2, head_pos.Y);
            if (bodyparts.Contains(p)) { k1++; }
            p = new Point(head_pos.X + 10 * 2, head_pos.Y);
            if (bodyparts.Contains(p)) { k1++; }
            p = new Point(head_pos.X, head_pos.Y - 10 * 2);
            if (bodyparts.Contains(p)) { k1++; }
            p = new Point(head_pos.X, head_pos.Y + 10 * 2);
            if (bodyparts.Contains(p)) { k1++; }


            if (k2 >= 2) { go = dodge3(); }
            else if (k1 >= 2) { dodge2(last_choice, head_pos); };
            if ((blocked) && (k1 >= 4)) { go = AI_2(); }
            else if (blocked) { go = dodge3(); }
            return go;
        }

        int dodge3()
        {

            clearmap();
            foreach(Point p in bodyparts)
            {map[p.X / 10, p.Y / 10, 0] = 999;}
            int go = -1;
            Point head3 = new Point(head_pos.X / 10, head_pos.Y / 10);
            rate_around(head3, 1);
            check_around(new Point(apple_pos.X / 10, apple_pos.Y / 10));
            go = map[head3.X, head3.Y, 1] - 1;
            if(go <= -1) { go = dodge4(-1); }
            return go;
        }

        //Rate cells, first step
        void rate_around(Point pos, int step)
        {
            List<Point> queue = new List<Point>();
            List<Point> queue2 = new List<Point>();

            map[pos.X, pos.Y, 0] = step;
            queue.Add(pos);

            while (queue.Count > 0)
            {

                for (int k=0;k<queue.Count;k++)
                {
                    Point cpos = queue[k];
                    //Dans chaque direction
                    foreach (Point dir in direction)
                    {

                        Point npos = new Point(dir.X / 10 + cpos.X, dir.Y / 10 + cpos.Y);
                        Point tnpos = new Point(npos.X * 10, npos.Y * 10);

                        //If new pos is valid
                        if ((inborder(tnpos))) /*|| (tnpos.Equals(head_pos))*/
                        {
                            //Console.WriteLine(map[npos.X, npos.Y, 0]);
                            //If pos is not rated
                            if ((map[npos.X, npos.Y, 0] == 0))
                            {


                                //Rate it
                                map[npos.X, npos.Y, 0] = step + 1;
                                queue2.Add(npos);

                                //if (step <= 100) { g.FillRectangle(crayony, tnpos.X, tnpos.Y, 9, 9); }
                                if (step < dij_radius) { g.FillRectangle(crayonc, tnpos.X, tnpos.Y, 6, 6); }
                                //else { g.FillRectangle(crayony, tnpos.X, tnpos.Y, 3, 3); }
                                if(tnpos.Equals(apple_pos))
                                {
                                    queue2.Clear();

                                    //alphbeta.Text += "Apple found"+" "+"; ";
                                    break;
                                }
                                //rate_around(npos, step + 1);
                            }
                        }
                    }
                }

                queue.Clear();
                foreach (Point p in queue2)
                { queue.Add(p); }
                queue2.Clear();
                step++;
                if (step > 3000) { alphbeta.Text+="Rate error; "; Console.WriteLine("Rating is too long, break"); break; }
            }
            queue.Clear();
            queue2.Clear();
        }


        //Read best path, second step
        void check_around(Point pos)
        {
            pos = new Point(pos.X, pos.Y);
            int best = map[pos.X, pos.Y, 0];
            //Console.WriteLine(best);
            //Console.WriteLine("And");
            Point npos;
            Point tnpos;
            Point cpos = pos;
            int c = -1;
            int last_c = c;
            int step = 1;

            //While head not found
            while ((best > 1) && (step < 10000))
            {
                //in each direction
                for (int k = 0; k < 4; k++)
                {
                    //Calculate new point coordinate
                    npos = new Point(direction[k].X / 10 + cpos.X, direction[k].Y / 10 + cpos.Y);
                    tnpos = new Point(npos.X * 10, npos.Y * 10);
                    //if path is in border and has a score lower than current
                    //Console.WriteLine(map[npos.X, npos.Y, 0].ToString());
                    if ((inborder(tnpos))
                    && (map[npos.X, npos.Y, 0] < best)
                    && (map[npos.X, npos.Y, 0] != 0))
                    {

                        //Remember direction
                        best = map[npos.X, npos.Y, 0];
                        c = k;
                    }
                }
                if ((c == -1))
                {
                    if (map[cpos.X, cpos.Y, 0] >= 3) {alphbeta.Text+="Path error; ";/*Console.WriteLine("Can't find head");*/ break; }

                    //Link path to head
                    Point head1 = head_pos;
                    Point tcpos = new Point(cpos.X*10,cpos.Y*10);

                    while (!head1.Equals(tcpos))
                    {
                        Point px = new Point(0, 0);
                        Point py = new Point(0, 0);
                        int x = head1.X - tcpos.X;
                        int y = head1.Y - tcpos.Y;
                        int Abx = Math.Abs(x);
                        int Aby = Math.Abs(y);
                        int sx = Math.Sign(x);
                        int sy = Math.Sign(y);
                        int vector1 = -1;

                        if (Abx > 0)
                        {
                            if ((sx > 0) && (!bodyparts.Contains(new Point(head1.X + direction[2].X, head1.Y + direction[2].Y)))) { vector1 = 2; }
                            if ((sx <= 0) && (!bodyparts.Contains(new Point(head1.X + direction[0].X, head1.Y + direction[0].Y)))) { vector1 = 0; }
                        }
                        if ((Aby > 0))
                        {
                            if ((sy > 0) && (!bodyparts.Contains(new Point(head1.X + direction[3].X, head1.Y + direction[3].Y)))) { vector1 = 3; }
                            if ((sy <= 0) && (!bodyparts.Contains(new Point(head1.X + direction[1].X, head1.Y + direction[1].Y)))) { vector1 = 1; }
                        }
                        if ((vector1 <= -1)) { debug.Text += "Can't link; "; map[head_pos.X/10, head_pos.Y/10, 1] = 0; break; }
                        else
                        {
                            map[head1.X/10, head1.Y/10, 1] = vector1 + 1;
                            //g.FillRectangle(crayonb, head1.X, head1.Y, 6, 6);

                            head1 = new Point(head1.X + direction[vector1].X, head1.Y + direction[vector1].Y);
                        }
                    }
                    break;
                }
                //Now take direction with the lower score
                cpos = new Point(direction[c].X / 10 + cpos.X, direction[c].Y / 10 + cpos.Y);
                //Console.WriteLine("Choice is : "+c.ToString());
                //mark this pos to point to old pos
                tnpos = new Point(cpos.X * 10, cpos.Y * 10);

                //g.FillRectangle(crayonb, tnpos.X, tnpos.Y, 6, 6);
                map[cpos.X, cpos.Y, 1] = (c + 2) % 4 + 1;
                last_c = c;
                c = -1;
                step++;
            }
            //printmap(1);
            //Console.WriteLine(map[cpos.X,cpos.Y,0] + " vs " + map[head_pos.X/10,head_pos.Y/10,0]);
        }

        int dodge1()
        {
            List<int> temp = new List<int>();
            int right = 0;
            int down = 0;
            int left = 0;
            int up = 0;
            temp.Add(right);
            temp.Add(down);
            temp.Add(left);
            temp.Add(up);
            Point future_pos;
            //foreach direction
            for (int i = 0; i < 4; i++)
            {
                future_pos = new Point(head_pos.X + direction[i].X, head_pos.Y + direction[i].Y);
                int len = lenght_path(future_pos, apple_pos, i);
                temp[i] += len;
            }
            //take the shortest path
            int output = -1;
            int go = -1;
            for (int i = 0; i < 4; i++)
            { if (temp[i] > go) { go = temp[i]; output = i; } }
            foreach(int o in temp) { Console.WriteLine(o.ToString()); }
            Console.WriteLine(output.ToString());
            return output;
        }

        int lenght_path(Point start, Point end,int csens)
        {
            //gauge.Value = 4;
            int lenght = 0;
            int x = start.X - end.X;
            int y = start.Y - end.Y;
            int Abx = Math.Abs(x);
            int Aby = Math.Abs(y);
            int sx = Math.Sign(x);
            int sy = Math.Sign(y);
            Point p = start;

            //g.FillRectangle(crayonr, start.X, start.Y, 3, 3);


            return lenght;
        }


        //dodgev2
        int dodge2(int go,Point head)
        {
            //gauge.Value = 20;
            List<int> temp = new List<int>();
            int right = 0;
            int down = 0;
            int left = 0;
            int up = 0;
            temp.Add(right);
            temp.Add(down);
            temp.Add(left);
            temp.Add(up);
            Point future_pos;
            for (int i=0;i<4;i++)
            {
                for (int k = 1; k < see_length; k++)
                {
                    future_pos = new Point(head.X+direction[i].X*k, head.Y+direction[i].Y*k);
                    //g.FillRectangle(crayonb, future_pos.X, future_pos.Y, 3, 3);
                    if ((!bodyparts.Contains(future_pos)) 
                    && inborder(future_pos))
                    {temp[i]++;}
                    else { break; }
                }
            }
            go = -1;
            int output=-1;
            //Console.WriteLine("-------ON--------");
            //Console.WriteLine("   "+ temp[3].ToString());
            //Console.WriteLine(temp[2].ToString() + "   " + temp[0].ToString());
            //Console.WriteLine("   "+temp[1].ToString());
            for (int i = 0; i < 4; i++) 
            { if (temp[i] > go) { go = temp[i]; output = i; } }
            //string dir = "";
            //if (output == 0) { dir = "right"; }
            //if (output == 1) { dir = "down"; }
            //if (output == 2) { dir = "left"; }
            //if (output == 3) { dir = "up"; }
            //Console.WriteLine(dir);
            //Console.WriteLine("-------" + f.ToString() + "--------");
            return output;
        }


        //Draw future direction
        void draw_tracker()
        {
            foreach (Point p in tracker)
            { g.FillRectangle(crayongray, p.X + 3, p.Y + 3, 4, 4); }
        }
        //clean tracker
        void clean_tracker()
        {
            foreach (Point p in tracker)
            {
                if (!bodyparts.Contains(p)) { g.FillRectangle(crayonw, p.X + 3, p.Y + 3, 4, 4); }
            }
            tracker.Clear();
        }

        //IA num 2
        //Q_learning based AI v1
        //Calculate score on new position
        int rate(Point p)
        {
            int ev_rate = 0;

            int dx_wall = Math.Min(d_points(new Point(p.X, 729), p), d_points(new Point(p.X, 0), p));
            int dy_wall = Math.Min(d_points(new Point(p.Y, 358), p), d_points(new Point(p.Y, 0), p));
            int d_wall = Math.Min(dx_wall, dy_wall);


            int d_apple = d_points(p, apple_pos);

            if (bodyparts.Contains(p)){ ev_rate -= 20; }
            else { ev_rate += 1; }

            if (d_apple <= 21) { ev_rate += 2; }

            if (p.Equals(apple_pos)) { ev_rate += 20; }

            if (blocked) { ev_rate -= 1; }
            else { ev_rate += 1; }

            if (d_wall <= 11) { ev_rate -= 1; }
            else { ev_rate += 1; }

            return ev_rate;
        }


        //called each frame
        int AI_2()
        {
            List<int> c_state = get_state(head_pos);
            float reward = 0f;
            int choice = proba(c_state);
            Point choosed_point =
                new Point(head_pos.X + direction[choice].X, head_pos.Y + direction[choice].Y);
            reward = rate(choosed_point);
            update_Q(choice, c_state, reward);
            //alphbeta.Text = "Alpha = " + alpha.ToString() + " Beta = " + beta.ToString()
            //+ "   Epsilon = " + epsilon.ToString();
            if(epsilon > 0f){ epsilon -= epsilon * 0.01f; }
            return choice;
        }


        //Give current state of snake
        List<int> get_state(Point cp)
        {
            List<int> status = new List<int>();
            int x = cp.X - apple_pos.X;
            int y = cp.Y - apple_pos.Y;
            float Abx = Math.Abs(x);
            float Aby = Math.Abs(y);
            float sx = Math.Sign(x);
            float sy = Math.Sign(y);
            if (Abx > Aby)
            {
                if (Abx >= 1)
                {
                    if (sx > 0)
                    { status.Add(2); }
                    else
                    { status.Add(0); }
                }
            }
            else
            {
                if (Aby >= 1)
                {
                    if (sy > 0)
                    { status.Add(3); }
                    else
                    { status.Add(1); }
                }
            }
            int dx_wall = Math.Min(d_points(new Point(cp.X, 729), cp), d_points(new Point(cp.X, 0), cp));
            int dy_wall = Math.Min(d_points(new Point(cp.Y, 358), cp), d_points(new Point(cp.Y, 0), cp));
            int d_wall = Math.Min(dx_wall, dy_wall);
            int d_apple = d_points(apple_pos, cp);
            //Console.WriteLine(d_apple.ToString());
            if (d_apple <= 21) { status.Add(4); }
            if (d_wall <= 11) { status.Add(5); }
            if (bodyparts.Contains(new Point(head_pos.X + direction[0].X,head_pos.Y + direction[0].Y))) { status.Add(6); }
            if (bodyparts.Contains(new Point(head_pos.X + direction[1].X, head_pos.Y + direction[1].Y))) { status.Add(7); }
            if (bodyparts.Contains(new Point(head_pos.X + direction[2].X, head_pos.Y + direction[2].Y))) { status.Add(8); }
            if (bodyparts.Contains(new Point(head_pos.X + direction[3].X, head_pos.Y + direction[3].Y))) { status.Add(9); }

            return status;
        }

        //get max q value
        float maxQ()
        {
            float max = 0;
            List<float> l = new List<float>();
            for (int i = 0; i < 4; i++)
            {
                Point prob_pos1 = new Point(head_pos.X + direction[i].X, head_pos.Y + direction[i].Y);
                if (!prob_pos1.Equals(last_pos))
                { l.Add(rate(prob_pos1)); }
            }
            foreach (float t in l) { max = Math.Max(max, t); }
            return max;
        }


        //return a point based on experience
        int proba(List<int> s)
        {
            //Console.WriteLine("error code proba");
            int choosed = random.Next(0,4);
            List<float> prop = new List<float>();
            float a=0, b=0, c=0, d = 0;
            foreach (int i in s)
            {
                a += Q_table[i, 0];
                b += Q_table[i, 1];
                c += Q_table[i, 2];
                d += Q_table[i, 3];
            }

            int tot = Convert.ToInt32(Math.Abs(a) + Math.Abs(b) + Math.Abs(c) + Math.Abs(d));

            prop.Add(a);
            prop.Add(b);
            prop.Add(c);
            prop.Add(d);
            float min = a;
            float sum=0;
            foreach(float n in prop) { if(n < min) { min = n; }}
            for (int l=0;l<prop.Count; l++)
            {prop[l] = (2*Math.Abs(min) + prop[l]) / tot;sum += prop[l]; }


            //foreach(float j in prop) { Console.WriteLine(j.ToString()); }

            float p1 = epsilon + prop[0];
            float p2 = epsilon + p1 + prop[1];
            float p3 = epsilon + p2 + prop[2];
            float p4 = epsilon + p3 + prop[3];

            float r = p4* (random.Next(0, 100) / 100f);

            if (r <= p1) { choosed = 0; }
            if ((r <= p2) && (r > p1)) { choosed = 1; }
            if ((r <= p3) && (r > p2)) { choosed = 2; }
            if ((r <= p4) && (r > p3)) { choosed = 3; }
            if (r > p4) { Console.WriteLine("what"); }
            return choosed;
        }

        //update Q table when AI choosed an option
        void update_Q(int choice, List<int> stat, float rwd)
        {
            float maxQf = maxQ();
            foreach (int k in stat)
            { Q_table[k, choice] += alpha * (rwd + beta * maxQf- Q_table[k, choice]); }
            //print_Q_table();
        }


        //print Q-table on console
        void print_Q_table()
        {
            for (int i = 0; i < Q_table.GetLongLength(0); i++)
            {
                float s1 = Q_table[i, 0];
                float s2 = Q_table[i, 1];
                float s3 = Q_table[i, 2];
                float s4 = Q_table[i, 3];
                Console.WriteLine("{0}, {1}, {2}, {3}", s1.ToString(), s2.ToString(), s3.ToString(), s4.ToString());
            }
            Console.WriteLine("--------------------------------------- ");
        }
        //###############################PART3_END#############################
    }
}