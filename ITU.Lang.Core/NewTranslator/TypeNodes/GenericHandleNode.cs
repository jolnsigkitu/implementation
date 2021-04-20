using System.Collections.Generic;
using ITU.Lang.Core.NewTranslator.TypeNodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.TypeNodes
{
    public class GenericHandleNode : TypeNode
    {
        private IEnumerable<string> Names;

        public GenericHandleNode(IEnumerable<string> names)
        {
            Names = names;
        }

        public override Type EvalType(Environment env)
        {
            throw new System.NotImplementedException();
        }
    }
}
