using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using IngameTextSurface = Sandbox.ModAPI.Ingame.IMyTextSurface;
using IngameCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;
using IngameIMyEntity = VRage.Game.ModAPI.Ingame.IMyEntity;
using VRage.Utils;
using VRage.Game.GUI.TextPanel;
using VRage.Game;
using Sandbox.Game.Entities;

namespace RedVsBlueClassSystem
{
    [MyTextSurfaceScript("GridStatusLCDScript", "Grid class status")]
    class GridStatusLCDScript : MyTSSCommon
    {
        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update10; // frequency that Run() is called.
        private readonly IMyTerminalBlock TerminalBlock;
        private int ScrollTime = 0;

        private static readonly float ScrollSpeed = 3;//pixels per update
        private static readonly int ScrollPauseUpdates = 18;//how many updates to say paused at the start and end when scrolling

        private CubeGridLogic GridLogic { get { return TerminalBlock?.GetGridLogic(); } }

        private Table HeaderTable = new Table() {
            Columns = new List<Column>() {
                new Column() { Name = "Label" },
                new Column() { Name = "Name" },
                new Column() { Name = "Success" }
            }
        };

        private Table GridResultsTable = new Table()
        {
            Columns = new List<Column>() {
                new Column() { Name = "Property" },
                new Column() { Name = "Value", Alignment = TextAlignment.RIGHT },
                new Column() { Name = "Separator" },
                new Column() { Name = "Max" },
                new Column() { Name = "Success" }
            }
        };

        private Table AppliedModifiersTable = new Table()
        {
            Columns = new List<Column>() {
                new Column() { Name = "ModifierName" },
                new Column() { Name = "Value" },
            }
        };

        public GridStatusLCDScript(IMyTextSurface surface, IngameCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            TerminalBlock = (IMyTerminalBlock)block; // internal stored m_block is the ingame interface which has no events, so can't unhook later on, therefore this field is required.
            TerminalBlock.OnMarkForClose += BlockMarkedForClose; // required if you're gonna make use of Dispose() as it won't get called when block is removed or grid is cut/unloaded.

            // Called when script is created.
            // This class is instanced per LCD that uses it, which means the same block can have multiple instances of this script aswell (e.g. a cockpit with all its screens set to use this script).
        }

        public override void Dispose()
        {
            base.Dispose(); // do not remove
            TerminalBlock.OnMarkForClose -= BlockMarkedForClose;

            // Called when script is removed for any reason, so that you can clean up stuff if you need to.
        }

        void BlockMarkedForClose(IngameIMyEntity ent)
        {
            Dispose();
        }

        // gets called at the rate specified by NeedsUpdate
        // it can't run every tick because the LCD is capped at 6fps anyway.
        public override void Run()
        {
            try
            {
                base.Run(); // do not remove

                /*// hold L key to see how the error is shown, remove this after you've played around with it =)
                if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.L))
                    throw new Exception("Oh noes an error :}");*/

                Draw();
            }
            catch (Exception e) // no reason to crash the entire game just for an LCD script, but do NOT ignore them either, nag user so they report it :}
            {
                DrawError(e);
            }
        }

        void Draw() // this is a custom method which is called in Run().
        {
            if(!Constants.IsClient)
            {
                return;
            }

            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenTopLeft = (Surface.TextureSize - screenSize) * 0.5f;
            Vector2 padding = new Vector2(10, 10);
            Vector2 cellGap = new Vector2(12, 5);
            float screenInnerWidth = Surface.SurfaceSize.X - (padding.X * 2);
            var SuccessColor = Color.Green;
            var FailColor = Color.Red;
            float baseScale = 1.25f;
            float titleScale = baseScale;
            float bodyScale = baseScale * 13 / TextUtils.CharWidth;

            var frame = Surface.DrawFrame();

            // https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites

            AddBackground(frame, Color.White.Alpha(0.05f));

            // the colors in the terminal are Surface.ScriptBackgroundColor and Surface.ScriptForegroundColor, the other ones without Script in name are for text/image mode.
            var gridClass = TerminalBlock.GetGridLogic().GridClass;

            if(gridClass == null)
            {
                return;
            }

            var checkGridResult = TerminalBlock.GetGridLogic().DetailedGridClassCheckResult;
            GridResultsTable.Clear();

            Vector2 currentPosition;
            List<MySprite> spritesToRender = new List<MySprite>();

            //Render the header
            HeaderTable.Clear();

            HeaderTable.Rows.Add(new Row()
            {
                new Cell("Class:"),
                new Cell(gridClass.Name, checkGridResult.Passed ? SuccessColor : FailColor),
                checkGridResult.Passed ? new Cell() : new Cell("X", FailColor)
            });

            HeaderTable.RenderToSprites(spritesToRender, screenTopLeft + padding, screenInnerWidth, new Vector2(15, 0), out currentPosition, titleScale);

            //Render the results checklist

            if (!checkGridResult.ValidGridType) {
                GridResultsTable.Rows.Add(new Row() {
                    new Cell("Grid type:"),
                    new Cell(null),
                    new Cell(null),
                    new Cell("Invalid", FailColor),
                    new Cell("X", FailColor),
                });
            }

            if (checkGridResult.MaxBlocks.Active || checkGridResult.MinBlocks.Active)
            {
                var passed = checkGridResult.MaxBlocks.Passed && checkGridResult.MinBlocks.Passed;
                string target = checkGridResult.MaxBlocks.Active && checkGridResult.MinBlocks.Active
                    ? $"{checkGridResult.MinBlocks.Limit} - {checkGridResult.MaxBlocks.Limit}"
                    : checkGridResult.MaxBlocks.Active
                        ? $"<= {checkGridResult.MaxBlocks.Limit}"
                        : $">= {checkGridResult.MinBlocks.Limit}";

                GridResultsTable.Rows.Add(new Row() {
                    new Cell("Blocks: "),
                    new Cell(checkGridResult.MaxBlocks.Value.ToString()),
                    new Cell("/"),
                    new Cell(target, passed ? SuccessColor : FailColor),
                    passed ? new Cell() : new Cell("X", FailColor),
                });
            }

            if (checkGridResult.MaxMass.Active)
            {
                GridResultsTable.Rows.Add(new Row() {
                    new Cell("Mass: "),
                    new Cell(checkGridResult.MaxMass.Value.ToString()),
                    new Cell("/"),
                    new Cell(checkGridResult.MaxMass.Limit.ToString(), checkGridResult.MaxMass.Passed ? SuccessColor : FailColor),
                    checkGridResult.MaxMass.Passed ? new Cell() : new Cell("X", FailColor),
                });
            }

            if (checkGridResult.MaxPCU.Active)
            {
                GridResultsTable.Rows.Add(new Row() {
                    new Cell("PCU: "),
                    new Cell(checkGridResult.MaxPCU.Value.ToString()),
                    new Cell("/"),
                    new Cell(checkGridResult.MaxPCU.Limit.ToString(), checkGridResult.MaxPCU.Passed ? SuccessColor : FailColor),
                    checkGridResult.MaxPCU.Passed ? new Cell() : new Cell("X", FailColor),
                });
            }

            if(gridClass.BlockLimits != null)
            {
                for (int i = 0; i < gridClass.BlockLimits.Length; i++)
                {
                    var blockLimit = gridClass.BlockLimits[i];
                    var checkResults = checkGridResult.BlockLimits[i];

                    GridResultsTable.Rows.Add(new Row() {
                        new Cell($"{blockLimit.Name}:"),
                        new Cell(checkResults.Score.ToString()),
                        new Cell("/"),
                        new Cell(checkResults.Max.ToString(), checkResults.Passed ? SuccessColor : FailColor),
                        checkResults.Passed ? new Cell() : new Cell("X", FailColor),
                    });
                }
            }
            
            Vector2 gridResultsTableTopLeft = currentPosition + new Vector2(0, 5);

            GridResultsTable.RenderToSprites(spritesToRender, gridResultsTableTopLeft, screenInnerWidth, cellGap, out currentPosition, bodyScale);

            //Applied modifiers
            spritesToRender.Add(CreateLine($"Applied modfiers", currentPosition + new Vector2(0, 5), out currentPosition, titleScale));

            AppliedModifiersTable.Clear();

            Vector2 appliedModifiersTableTopLeft = currentPosition + new Vector2(0, 5);

            foreach(var modifierValue in GridLogic.Modifiers.GetModifierValues())
            {
                AppliedModifiersTable.Rows.Add(new Row()
                {
                    new Cell($"{modifierValue.Name}:"),
                    new Cell(modifierValue.Value.ToString()),
                });
            }

            AppliedModifiersTable.RenderToSprites(spritesToRender, appliedModifiersTableTopLeft, screenInnerWidth, cellGap, out currentPosition, bodyScale);

            Vector2 scrollPosition = GetScrollPosition(currentPosition + padding);

            for(int i = 0; i < spritesToRender.Count; i++)
            {
                var sprite = spritesToRender[i];

                if (scrollPosition.Y != 0) {
                    sprite.Position = sprite.Position - scrollPosition;
                }

                frame.Add(sprite);
            }

            frame.Dispose(); // send sprites to the screen
        }

        private Vector2 GetScrollPosition(Vector2 contentBottomRight)
        {
            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenTopLeft = (Surface.TextureSize - screenSize) * 0.5f;
            var contentHeight = contentBottomRight.Y - screenTopLeft.Y;

            if (contentHeight > screenSize.Y)
            {
                float scrollRange = contentHeight - screenSize.Y;
                int numUpdatesToScroll = (int)Math.Ceiling(scrollRange / ScrollSpeed);
                int fullScrollCycleTime = (ScrollPauseUpdates + numUpdatesToScroll) * 2;
                Vector2 scrollPosition;

                if(ScrollTime < ScrollPauseUpdates)
                {
                    scrollPosition  = new Vector2();
                }
                else if(ScrollTime < ScrollPauseUpdates + numUpdatesToScroll)
                {
                    scrollPosition = new Vector2(0, (ScrollTime - ScrollPauseUpdates) * ScrollSpeed);
                }
                else if(ScrollTime < ScrollPauseUpdates * 2 + numUpdatesToScroll)
                {
                    scrollPosition = new Vector2(0, scrollRange);
                } else
                {
                    scrollPosition = new Vector2(0, scrollRange - (ScrollTime - (ScrollPauseUpdates * 2 + numUpdatesToScroll)) * ScrollSpeed);
                }

                ScrollTime++;

                if(ScrollTime > fullScrollCycleTime)
                {
                    ScrollTime = 0;
                }

                return scrollPosition;
            }

            return new Vector2();
        }

        MySprite CreateLine(string text, Vector2 position, float scale = 1)
        {
            var ignored = new Vector2();

            return CreateLine(text, position, out ignored, scale);
        }

        MySprite CreateLine(string text, Vector2 position, out Vector2 positionAfter, float scale = 1)
        {
            var sprite = MySprite.CreateText(text, "Monospace", Color.White, scale, TextAlignment.LEFT);
            sprite.Position = position;// screenCorner + padding + new Vector2(0, y); // 16px from topleft corner of the visible surface

            positionAfter = position + new Vector2(0, TextUtils.GetTextHeight(text, scale));

            return sprite;
        }

        void DrawError(Exception e)
        {
            Utils.Log($"{e.Message}\n{e.StackTrace}", 3);

            try // first try printing the error on the LCD
            {
                Vector2 screenSize = Surface.SurfaceSize;
                Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

                var frame = Surface.DrawFrame();

                var bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", null, null, Color.Black);
                frame.Add(bg);

                var text = MySprite.CreateText($"ERROR: {e.Message}\n{e.StackTrace}\n\nPlease send screenshot of this to mod author.\n{MyAPIGateway.Utilities.GamePaths.ModScopeName}", "White", Color.Red, 0.7f, TextAlignment.LEFT);
                text.Position = screenCorner + new Vector2(16, 16);
                frame.Add(text);

                frame.Dispose();
            }
            catch (Exception e2)
            {
                MyLog.Default.WriteLineAndConsole($"Also failed to draw error on screen: {e2.Message}\n{e2.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }
    }
}
