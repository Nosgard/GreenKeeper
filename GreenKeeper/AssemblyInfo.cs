using System.Runtime.CompilerServices;
using System.Windows;

// Lets the test assembly reach the internal ToDueDateText overload that takes an
// explicit reference date. Without a pinned "today", calendar edge cases such as
// month ends and leap days can only be reproduced on a few days per year, which
// would leave those regressions untestable.
[assembly: InternalsVisibleTo("GreenKeeper.Tests")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]
