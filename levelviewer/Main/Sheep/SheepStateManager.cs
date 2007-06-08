using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sheep
{
    class SheepStateManager
    {
        public Parameter PopStack() { return _stack.Pop(); }

        public void PushStack(Parameter param)
        {
            _stack.Push(param);
        }

        public void PushStack(int i)
        {
            Parameter p = new Parameter();
            p.Type = ParameterType.Integer;
            p.Integer = i;

            _stack.Push(p);
        }

        public void PushStack(float f)
        {
            Parameter p = new Parameter();
            p.Type = ParameterType.Float;
            p.Float = f;

            _stack.Push(p);
        }

        public void PushString(int offset)
        {
            Parameter p = new Parameter();
            p.Type = ParameterType.String;
            p.Integer = offset;

            _stack.Push(p);
        }

        public Parameter PeekStack() { return _stack.Peek(); }

        private static Stack<Parameter> _stack = new Stack<Parameter>();
    }
}
