﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xamarin.Forms;

namespace Tricycle.UI.Tests
{
    [TestClass]
    public class AppManagerTests
    {
        AppManager _appManager;

        [TestInitialize]
        public void Setup()
        {
            _appManager = new AppManager();
        }

        [TestMethod]
        public void RaiseReadyRaisesReadyEvent()
        {
            bool raised = false;

            _appManager.Ready += () => raised = true;
            _appManager.RaiseReady();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void RaiseBusyRaisesBusyEvent()
        {
            bool raised = false;

            _appManager.Busy += () => raised = true;
            _appManager.RaiseBusy();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void RaiseBusySetsIsBusy()
        {
            _appManager.RaiseBusy();

            Assert.IsTrue(_appManager.IsBusy);
        }

        [TestMethod]
        public void RaiseReadyResetsIsBusy()
        {
            _appManager.RaiseBusy();

            if (!_appManager.IsBusy)
            {
                Assert.Inconclusive("IsBusy was not set by RaiseBusy.");
            }

            _appManager.RaiseReady();

            Assert.IsFalse(_appManager.IsBusy);
        }

        [TestMethod]
        public void RaiseFileOpenedRaisesFileOpenedEvent()
        {
            string actual = null;
            string expected = "/Users/fred/Movies/movie.mkv";

            _appManager.FileOpened += fileName => actual = fileName;
            _appManager.RaiseFileOpened(expected);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RaiseQuittingRaisesQuittingEvent()
        {
            bool raised = false;

            _appManager.Quitting += () => raised = true;
            _appManager.RaiseQuitting();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void RaiseQuitConfirmedRaisesQuitConfirmedEvent()
        {
            bool raised = false;

            _appManager.QuitConfirmed += () => raised = true;
            _appManager.RaiseQuitConfirmed();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void RaiseQuitConfirmedSetsIsQuitConfirmed()
        {
            _appManager.RaiseQuitConfirmed();

            Assert.IsTrue(_appManager.IsQuitConfirmed);
        }

        [TestMethod]
        public void RaiseModalOpenedRaisesModalOpenedEvent()
        {
            Page actual = null;
            Page expected = new Page();

            _appManager.ModalOpened += page => actual = page;
            _appManager.RaiseModalOpened(expected);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RaiseModalOpenedSetsIsModalOpened()
        {
            _appManager.RaiseModalOpened(null);

            Assert.IsTrue(_appManager.IsModalOpen);
        }

        [TestMethod]
        public void RaiseModalClosedRaisesModalClosedEvent()
        {
            bool raised = false;

            _appManager.ModalClosed += () => raised = true;
            _appManager.RaiseModalClosed();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void RaiseModalClosedResetsIsModalOpen()
        {
            _appManager.RaiseModalOpened(null);

            if (!_appManager.IsModalOpen)
            {
                Assert.Inconclusive("IsModalOpen was not set by RaiseModalOpened.");
            }

            _appManager.RaiseModalClosed();

            Assert.IsFalse(_appManager.IsModalOpen);
        }

        [TestMethod]
        public void RaiseModalClosedDoesNotResetIsModalOpenWhenAdditionalModalsAreOpen()
        {
            _appManager.RaiseModalOpened(null);
            _appManager.RaiseModalOpened(null);

            if (!_appManager.IsModalOpen)
            {
                Assert.Inconclusive("IsModalOpen was not set by RaiseModalOpened.");
            }

            _appManager.RaiseModalClosed();

            Assert.IsTrue(_appManager.IsModalOpen);
        }
    }
}