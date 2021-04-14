using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using ITU.Lang.StandardLib;

namespace ITU.Lang.SampleProgram.Lib
{
    public class JSKeyboardEventHandlerSignal
    {
        private Action<KeyboardEventArgs> producerPush;

        public async Task<ProducerSignal<KeyboardEventArgs>> Attach(IJSRuntime jsRuntime, string queryString)
        {
            await jsRuntime.InvokeVoidAsync("AddKeyupEventListenerCSharp", DotNetObjectReference.Create(this), queryString);

            return PushSignal<KeyboardEventArgs>.Produce((push) =>
            {
                producerPush = push;
            });
        }

        [JSInvokable]
        public void EventHandler(KeyboardEventArgs evt)
        {
            if (producerPush != null)
            {
                producerPush(evt);
            }
        }

        private void InternalProducer(Action<KeyboardEventArgs> push)
        {
            this.producerPush = push;
        }
    }
}
