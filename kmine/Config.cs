using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace kmine
{
    public class Config
    {
        public string WindowName { get; set; }
        public int xOffset { get; set; }
        public int yOffset { get; set; }
        public int maxWidth { get; set; }
        public int maxHeight { get; set; }
        protected Hashtable images { get; set; }

        public GImage this[string name] { get { if (images.ContainsKey(name)) return (GImage)images[name]; return null; } }

        public Config()
        {
            xOffset = GetOffset(global::kmine.Properties.Settings.Default.x_offset);
            yOffset = GetOffset(global::kmine.Properties.Settings.Default.y_offset);
            maxWidth = 0;
            maxHeight = 0;
            images = new Hashtable();
        }

        private int GetOffset(string p)
        {
            int x = -1;
            if (string.IsNullOrWhiteSpace(p) || p.ToLower().Equals("auto") || !int.TryParse(p, out x))
                return -1;
            return x;
        }

        /// <summary>
        /// Построчное чтение конфига
        /// </summary>
        /// <param name="s">Строка конфигурации</param>
        public void ReadConfig(string s)
        {
            var ts = s.TrimStart();
            if (!ts.StartsWith("#") && !ts.StartsWith("//"))
            {
                var l = ts.Split('=');
                if (l.Length > 1)
                {
                    var tl = l[1].Trim();
                    switch (l[0].TrimEnd())
                    {
                        case @"window":
                            WindowName = tl;
                            break;
                        case @"x-offset":
                            {
                                int tmp;
                                if (int.TryParse(tl, out tmp))
                                    xOffset = tmp;
                            }
                            break;
                        case @"y-offset":
                            {
                                int tmp;
                                if (int.TryParse(tl, out tmp))
                                    yOffset = tmp;
                            }
                            break;
                        case @"max-width":
                            {
                                int tmp;
                                if (int.TryParse(tl, out tmp))
                                    maxWidth = tmp;
                            }
                            break;
                        case @"max-height":
                            {
                                int tmp;
                                if (int.TryParse(tl, out tmp))
                                    maxHeight = tmp;
                            }
                            break;
                        case @"image":
                            {
                                string[] tmp = tl.Split(':');
                                if (tmp.Length > 1)
                                {
                                    try
                                    {
                                        images[tmp[0].Trim()] = new GImage(tmp[1].Trim());
                                    }
                                    catch (Exception e) { Program.log.Error("Error loading image {0}, {1}", s, e.Message); }
                                }
                                else Program.log.Error("Bad image {0}", s);
                            }
                            break;
                        default:
                            Program.log.Error("Bad config line '{0}'", s);
                            break;
                    }
                }
            }
        }
    }
}