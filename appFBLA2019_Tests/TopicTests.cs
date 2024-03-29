﻿using System;
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
            // TO DO: Pass in username of level atuhor
            //appFBLA2019.Level topic = new Level("TestTopic.txt");

            //Assert.IsTrue(topic.Title == "TestTopic - Technology");

            //appFBLA2019.Question question = topic.GetQuestion();
            //Assert.IsTrue(question.QuestionText == "What is a phone?");
            //Assert.IsTrue(question.CorrectAnswer == "A tool");
        }

        [TestMethod]
        public void EmptyQuestion()
        {
            appFBLA2019.Question question = new appFBLA2019.Question();

            Assert.IsTrue(question.QuestionText == "This question is empty!");
            Assert.IsTrue(question.CorrectAnswer == null);
        }
    }
}
