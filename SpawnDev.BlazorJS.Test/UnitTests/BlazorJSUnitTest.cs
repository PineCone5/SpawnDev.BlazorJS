﻿using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Test.Services;

namespace SpawnDev.BlazorJS.Test.UnitTests {
    public class BlazorJSUnitTest {
        public interface IWindow {
            string Name { get; set; }
        }

        [TestMethod]
        public void IJSObjectInterfacePropertySetGet() {
            var w = JS.Get<IWindow>("window");
            var randName = Guid.NewGuid().ToString();
            w.Name = randName;
            if (w.Name != randName) throw new Exception("Interface property set/get failed");
        }

        [TestMethod]
        public void JSObjectPropertySetGet() {
            var w = JS.Get<Window>("window");
            var randName = Guid.NewGuid().ToString();
            w.Name = randName;
            if (w.Name != randName) throw new Exception("Interface property set/get failed");
        }

        [TestMethod]
        public void IJSObjectInterfaceArraySetGet() {
            var w = JS.Get<IWindow>("window");
            var randName = Guid.NewGuid().ToString();
            w.Name = randName;
            var array = new IWindow[] { w, w, w };
            JS.Set("_array", array);
            var arrayReadBack = JS.Get<IWindow[]>("_array");
            if (arrayReadBack.ToList().Where(w => w.Name != randName).Any()) throw new Exception("Interface array set/get failed");
        }

        // Promise

        [TestMethod]
        public async Task JSObjectPromiseResolve() {
            var randValue = Guid.NewGuid().ToString();
            using var promise = new Promise<string>();
            _ = Task.Run(async () => {
                await Task.Delay(200);
                promise.Resolve(randValue);
            });
            var returnedValue = await promise.ThenAsync();
            if (returnedValue != randValue) throw new Exception("Promise return value mismatch");
        }

        [TestMethod]
        public async Task JSObjectPromiseFromTask() {
            var randValue = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            using var promise = new Promise<string>(tcs.Task);
            _ = Task.Run(async () => {
                await Task.Delay(200);
                tcs.TrySetResult(randValue);
            });
            var returnedValue = await promise.ThenAsync();
            if (returnedValue != randValue) throw new Exception("Promise return value mismatch");
        }

        [TestMethod]
        public async Task JSObjectPromiseFromLambda() {
            var randValue = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            using var promise = new Promise<string>(async () => {
                await Task.Delay(200);
                return randValue;
            });
            var returnedValue = await promise.ThenAsync();
            if (returnedValue != randValue) throw new Exception("Promise return value mismatch");
        }

        [TestMethod]
        public async Task JSObjectPromiseResolveVoid() {
            var randValue = Guid.NewGuid().ToString();
            using var promise = new Promise();
            _ = Task.Run(async () => {
                await Task.Delay(200);
                promise.Resolve(randValue);
            });
            await promise.ThenAsync();
        }

        [TestMethod]
        public async Task JSObjectPromiseFromTaskVoid() {
            var randValue = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource();
            using var promise = new Promise(tcs.Task);
            _ = Task.Run(async () => {
                await Task.Delay(200);
                tcs.TrySetResult();
            });
            await promise.ThenAsync();
        }

        [TestMethod]
        public async Task JSObjectPromiseFromLambdaVoid() {
            var randValue = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource();
            using var promise = new Promise(async () => {
                await Task.Delay(200);
            });
            await promise.ThenAsync();
        }
    }
}
