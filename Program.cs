

using AltiumSharpTest;

namespace AltiumScharpTest {
    class Program {
        public static void Main(string[] args) {
            if (args.Count() != 1) {
                return;
            }
            Application app = new Application(args[0]);

            app.run();
        }
    }
}