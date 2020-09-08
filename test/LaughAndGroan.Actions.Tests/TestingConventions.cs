namespace LaughAndGroan.Actions.Tests
{
    using Fixie;

    public class DiscoveryConvention : Discovery
    {
        public DiscoveryConvention()
        {
            Methods
                .Where(x => x.Name != "SetUp");
        }
    }

    public class ExecutionConvention : Execution
    {
        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                SetUp(instance);

                @case.Execute(instance);
            });
        }

        static void SetUp(object instance)
        {
            instance.GetType().GetMethod("SetUp")?.Execute(instance);
        }
    }
}
