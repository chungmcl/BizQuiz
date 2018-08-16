using System;
using appFBLA2019;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace appFBLA2019_Tests
{
    [TestClass]
    public class TopicTests
    {
        [TestMethod]
        public void LoadTopic()
        {
            appFBLA2019.Topic topic = new Topic("TestTopic.txt");

            Assert.IsTrue(topic.title == "TestTopic - Technology");

            topic.Set();
            Assert.IsTrue(topic.GetQuestion().GetType() == typeof(Question));
            topic.Set();
            Assert.IsTrue(topic.GetQuestion().QuestionText == "What is a phone?");
            topic.Set();
            Assert.IsTrue(topic.GetQuestion().correctAnswer == "A tool");
        }
    }
}
