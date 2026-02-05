using Budgetr.Shared.Models;
using System;

try
{
    Console.WriteLine("Testing Meter Factor Validation...");

    // Test 1: Valid positive factor
    var m1 = new Meter { Name = "Valid+", Factor = 1.0 };
    Console.WriteLine($"Test 1 (Valid 1.0): PASS");

    // Test 2: Valid negative factor
    var m2 = new Meter { Name = "Valid-", Factor = -10.0 };
    Console.WriteLine($"Test 2 (Valid -10.0): PASS");

    // Test 3: Valid max factor
    var m3 = new Meter { Name = "ValidMax", Factor = 10.0 };
    Console.WriteLine($"Test 3 (Valid 10.0): PASS");

    // Test 4: Invalid positive factor
    try
    {
        var m4 = new Meter { Name = "Invalid+", Factor = 10.1 };
        Console.WriteLine($"Test 4 (Invalid 10.1): FAIL (Expected ArgumentOutOfRangeException)");
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine($"Test 4 (Invalid 10.1): PASS (Caught expected exception)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test 4 (Invalid 10.1): FAIL (Caught wrong exception: {ex.GetType().Name})");
    }

    // Test 5: Invalid negative factor
    try
    {
        var m5 = new Meter { Name = "Invalid-", Factor = -10.1 };
        Console.WriteLine($"Test 5 (Invalid -10.1): FAIL (Expected ArgumentOutOfRangeException)");
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine($"Test 5 (Invalid -10.1): PASS (Caught expected exception)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test 5 (Invalid -10.1): FAIL (Caught wrong exception: {ex.GetType().Name})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled exception: {ex}");
}
