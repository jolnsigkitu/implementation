using System;
using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class GenericFunctionWrapper : GenericWrapper, IFunctionType
    {
        public new IFunctionType Child { get; set; }

        public GenericFunctionWrapper(IFunctionType child, IEnumerable<string> bindings) : base(child, bindings)
        {
            Child = child;
        }

        public GenericFunctionWrapper(IFunctionType child, IDictionary<string, IType> bindings, IList<string> handle) : base(child, bindings, handle)
        {
            Child = child;
        }

        public override IFunctionType ResolveByIdentifier(IDictionary<string, IType> resolutions)
        {
            var newBindings = ResolveBindingsByIdentifier(resolutions);

            return new GenericFunctionWrapper(Child, newBindings, Handle);
        }

        public override IType ResolveByPosition(IEnumerable<IType> resolutions)
        {
            throw new System.NotImplementedException("ResolveByPosition in GenericFunctionWrapper");
            // return base.ResolveByPosition(resolutions);
        }

        public IFunctionType ResolveByParameterPosition(IEnumerable<IType> expressions)
        {
            var resolutions = new Dictionary<string, IType>();
            expressions.Zip(ParameterTypes)
                .ForEach(
                    pair => MatchTypes(pair, new List<(string, IType)>())
                        .ForEach(pair => resolutions.TryAdd(pair.Key, pair.Value))
                );

            return ResolveByIdentifier(resolutions);
            throw new System.NotImplementedException("ResolveByParameterPosition in GenericFunctionWrapper");
            // return base.ResolveByPosition(resolutions);
        }

        private IList<(string Key, IType Value)> MatchTypes(
            (IType expr, IType parameter) pair,
            IList<(string, IType)> list
        )
        {
            if (pair.parameter is GenericTypeIdentifier gti)
            {
                list.Add((gti.Identifier, pair.expr));
            }

            if (pair.expr is IFunctionType exprFt && pair.parameter is IFunctionType paramFt)
            {
                MatchTypes((exprFt.ReturnType, paramFt.ReturnType), list);
                exprFt.ParameterTypes.Zip(paramFt.ParameterTypes).ForEach(pair => MatchTypes(pair, list));
            }

            return list;
        }

        public override string AsNativeName() => Child.AsNativeName();

        public override string AsTranslatedName() => Child.AsTranslatedName();

        public override string ToString() => $"(FunctionWrapper - Handle: {Handle}, Child: {Child})";

        #region Equality
        public override bool Equals(IType other)
        {
            return other is GenericFunctionWrapper wrapper &&
                   EqualityComparer<IDictionary<string, IType>>.Default.Equals(Bindings, wrapper.Bindings) &&
                   EqualityComparer<IType>.Default.Equals(Child, wrapper.Child) &&
                   EqualityComparer<IList<string>>.Default.Equals(Handle, wrapper.Handle) &&
                   EqualityComparer<IFunctionType>.Default.Equals(Child, wrapper.Child) &&
                   IsLambda == wrapper.IsLambda &&
                   EqualityComparer<IType>.Default.Equals(ReturnType, wrapper.ReturnType) &&
                   EqualityComparer<IList<IType>>.Default.Equals(ParameterTypes, wrapper.ParameterTypes) &&
                   EqualityComparer<IEnumerable<string>>.Default.Equals(ParameterNames, wrapper.ParameterNames);
        }

        public override int GetHashCode() => HashCode.Combine(
            Bindings,
            Child,
            Handle,
            Child,
            IsLambda,
            ReturnType,
            ParameterTypes,
            ParameterNames
        );
        #endregion

        #region IFunctionType props
        public bool IsLambda
        {
            get => Child.IsLambda;
            set => Child.IsLambda = value;
        }
        public IType ReturnType
        {
            get => TryResolveType(Child.ReturnType);
            set => Child.ReturnType = value;
        }
        public IList<IType> ParameterTypes
        {
            get => Child.ParameterTypes.Select(TryResolveType).ToList();
            set => Child.ParameterTypes = value;
        }
        public IEnumerable<string> ParameterNames
        {
            get => Child.ParameterNames;
            set => Child.ParameterNames = value;
        }
        #endregion
    }
}
