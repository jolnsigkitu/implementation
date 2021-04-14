using System;
using ITU.Lang.StandardLib;
using Microsoft.AspNetCore.Components.Web;

class Engine
{
    public void init(ProducerSignal<KeyboardEventArgs> signal)
    {
        signal.ForEach((evt) =>
        {
            Console.WriteLine("From engine");
            Console.WriteLine(evt.Key);
        });
    }
    public string getState() => "foobar";
}
