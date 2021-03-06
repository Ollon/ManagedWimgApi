﻿using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class RemountImageTests : TestBase
    {
        [Test]
        public void RemountImageTest()
        {
            using (var imageHandle = WimgApi.LoadImage(TestWimHandle, 1))
            {
                WimgApi.MountImage(imageHandle, MountPath, WimMountImageOptions.Fast | WimMountImageOptions.DisableDirectoryAcl | WimMountImageOptions.DisableFileAcl | WimMountImageOptions.DisableRPFix | WimMountImageOptions.ReadOnly);

                try
                {
                    VerifyMountState(imageHandle, WimMountPointState.Mounted);

                    KillWimServ();

                    VerifyMountState(imageHandle, WimMountPointState.Remountable);

                    WimgApi.RemountImage(MountPath);

                    VerifyMountState(imageHandle, WimMountPointState.Mounted);
                }
                finally
                {
                    WimgApi.UnmountImage(imageHandle);
                }
            }
        }

        [Test]
        public void RemountImageTest_ThrowsArgumentNullException_mountPath()
        {
            ShouldThrow<ArgumentNullException>("mountPath", () =>
                WimgApi.RemountImage(null));
        }

        [Test]
        public void RemountImageTest_ThrowsDirectoryNotFoundException_mountPath()
        {
            Should.Throw<DirectoryNotFoundException>(() =>
                WimgApi.RemountImage(Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString())));
        }

        private void KillWimServ()
        {
            var wimServProcesses = Process.GetProcessesByName("wimserv");

            wimServProcesses.Length.ShouldNotBe(0);

            foreach (var process in wimServProcesses)
            {
                process.Kill();
            }
        }

        private void VerifyMountState(WimHandle imageHandle, WimMountPointState expectedMountPointState)
        {
            var mountedImageInfo = WimgApi.GetMountedImageInfoFromHandle(imageHandle);

            (mountedImageInfo.State | expectedMountPointState).ShouldBe(expectedMountPointState);
        }
    }
}