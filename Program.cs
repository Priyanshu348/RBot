using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Net.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DSharpPlus.Entities;
using System.IO;

namespace RBot
{
    class Commands
    {
        Thread RainbowThread = new Thread(new ParameterizedThreadStart(Recolor));
        static CommandContext cttx;
        static System.Timers.Timer timer = new System.Timers.Timer(10);
        static float prog = 0;
        static float dprog = 0.0175F;
        static DiscordColor defcol;

        [Command("reset")]
        public async Task reset(CommandContext ctx)
        {
            timer.Stop();
            RainbowThread.Abort();
            await ctx.Guild.UpdateRoleAsync(ctx.Member.Roles.FirstOrDefault(), color: defcol);
        }

        [Command("exit")]
        public async Task exit(CommandContext ctx)
        {
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }

        [Command("rainbow")]
        public async Task rainbow(CommandContext ctx, float ddprog = -1)
        {
            defcol = ctx.Member.Color;
            if (ddprog != -1 && ddprog > 0 && ddprog < 0.5)
            {
                dprog = ddprog;
            }
            RainbowThread.Start(ctx);
        }

        public static void Recolor(object e)
        {
            cttx = (CommandContext)e;
            timer.Elapsed += Elapsed;
            timer.Start();
        }

        private static void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DiscordColor col = Rainbow(prog);
            prog += dprog;
            cttx.Guild.UpdateRoleAsync(cttx.Member.Roles.FirstOrDefault(), color: col);
        }

        public static DiscordColor Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;
            DiscordColor col;
            switch ((int)div)
            {
                case 0:
                    col = new DiscordColor(255 / 255, (float)ascending / 255, 0);
                    break;
                case 1:
                    col = new DiscordColor((float)descending / 255, 255 / 255, 0);
                    break;
                case 2:
                    col = new DiscordColor(0, 255 / 255, (float)ascending / 255);
                    break;
                case 3:
                    col = new DiscordColor(0, (float)descending / 255, 255 / 255);
                    break;
                case 4:
                    col = new DiscordColor((float)ascending / 255, 0, 255 / 255);
                    break;
                default: 
                    col = new DiscordColor(255 / 255, 0, (float)descending / 255);
                    break;
            }
            return col;
        }

        [Command("getcol")]
        public async Task getcol(CommandContext ctx, string rName = null)
        {
            if (rName == null)
            {
                DiscordColor col = ctx.Member.Color;
                await ctx.RespondAsync(ctx.Member.Mention + ", your color is: R:" + col.R + ", G:" + col.G + ", B:" + col.B + ", val:" + col.Value);
            }
        }

        [Command("setcol")]
        public async Task setcol(CommandContext ctx, int val, int g = -1, int b = -1)
        {
            if (val > 255 && g == -1 && b == -1)
            {
                DiscordColor col = new DiscordColor(val);
                await ctx.Guild.UpdateRoleAsync(ctx.Member.Roles.FirstOrDefault(), color: col);
            }
            else
            {
                float R = val;
                float G = g;
                float B = b;
                DiscordColor col = new DiscordColor(R / 255, G / 255, B / 255);
                await ctx.Guild.UpdateRoleAsync(ctx.Member.Roles.FirstOrDefault(), color: col);
            }
        }
    }

    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule cmd;
        static string token = null;

        static void Main(string[] args)
        {
            if (!File.Exists("config.txt"))
            {
                Console.WriteLine("config.txt does not exists, leaving...");
                Console.ReadKey();
            }
            else
            {
                token = File.ReadAllText("config.txt");
                MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            cmd = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ">"
            });

            discord.SetWebSocketClient<WebSocket4NetClient>();
            cmd.RegisterCommands<Commands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
