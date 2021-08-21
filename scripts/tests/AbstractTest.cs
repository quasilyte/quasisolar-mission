using System;
using Godot;

public abstract class AbstractTest {
    protected abstract void RunTest();

    public TestingResult Run() {
        var result = new TestingResult();
        try {
            RunTest();
        } catch (TestFailureException e) {
            result.err = new TestingResult.TestError{
                message = e.Message,
                stackTrace = e.StackTrace,
            };
        }
        return result;
    }

    class TestFailureException : Exception {
        public TestFailureException(string message): base(message) {}
    }

    protected void Error(string msg) {
        throw new TestFailureException(msg);
    }
}
