using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace ProjectEstimate.Agents.Consultant.Plugins;

public class CalculatorPlugin
{
    [KernelFunction("add")]
    [Description("Adds two numbers together")]
    public ValueTask<double> AddAsync(double a, double b)
    {
        return ValueTask.FromResult(a + b);
    }

    [KernelFunction("subtract")]
    [Description("Subtracts two numbers")]
    public ValueTask<double> SubtractAsync(double a, double b)
    {
        return ValueTask.FromResult(a - b);
    }

    [KernelFunction("multiply")]
    [Description("Multiplies two numbers")]
    public ValueTask<double> MultiplyAsync(double a, double b)
    {
        return ValueTask.FromResult(a * b);
    }

    [KernelFunction("divide")]
    [Description("Divides two numbers")]
    public ValueTask<double> DivideAsync(double a, double b)
    {
        return ValueTask.FromResult(a / b);
    }
}
