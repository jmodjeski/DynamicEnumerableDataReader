using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;

namespace ModjeskiNet.Data 
{
    public class PredicateBuilder<TArg> 
    {
        private static readonly char[] Operators = new[]{
            '!','=','>','<','&','|'};
        private static readonly Dictionary<string, ExpressionType> OperatorMap = new Dictionary<string, ExpressionType>
        {
            {"!", ExpressionType.Not},
            {"!=", ExpressionType.NotEqual},
            {"==", ExpressionType.Equal},
            {">", ExpressionType.GreaterThan},
            {">=", ExpressionType.GreaterThanOrEqual},
            {"<", ExpressionType.LessThanOrEqual},
            {"<=", ExpressionType.LessThanOrEqual},
            {"&&", ExpressionType.AndAlso},
            {"||", ExpressionType.OrElse},

        };

        private ParameterExpression m_root = Expression.Parameter(typeof(TArg));
        public Func<TArg, bool> Build(string expression)
        {
            var index = 0;
            var predicate = Parse(expression, ref index);

            return Expression.Lambda<Func<TArg, bool>>(
                predicate, new[] { m_root }).Compile();
        }

        private Expression Parse(string expression, ref int index)
        {
            var operators = new Stack<ExpressionType>();
            var current = (Expression)null;

            for (; index < expression.Length; index++)
            {
                if (Char.IsWhiteSpace(expression[index]))
                {
                    continue; // swallow whitespace
                }

                if (Operators.Contains(expression[index]))
                {
                    var op = ParseOperator(expression, ref index);
                    operators.Push(op);
                    continue;
                }

                if(expression[index] == '(')
                {
                    current = Combine(current, operators, ParseGroup(expression, ref index));
                }
                else if(expression[index] == ')')
                {
                    break;
                }

                if (expression[index] == '[')
                {
                    current = Combine(current, operators, ParseName(expression, ref index));
                    continue;
                }

                if (expression[index] == '\'')
                {
                    current = Combine(current, operators, ParseString(expression, ref index));
                    continue;
                }

                if (expression[index] == '#')
                {
                    current = Combine(current, operators, ParseDate(expression, ref index));
                    continue;
                }

                if (Char.IsNumber(expression[index]))
                {
                    current = Combine(current, operators, ParseNumber(expression, ref index));
                    continue;
                }
            }

            return current;
        }

        private Expression ParseGroup(string expression, ref int index)
        {
            ++index;
            return Parse(expression, ref index);
        }

        private Expression Combine(Expression current, Stack<ExpressionType> operators, Expression expression)
        {
            if (current == null && !operators.Any())
                return expression;

            foreach (var op in operators)
            {
                var left = current;
                var right = expression;

                if(op != ExpressionType.OrElse && op != ExpressionType.AndAlso)
                    if (left.Type != expression.Type)
                        left = Expression.Convert(left, expression.Type);

                if (left != null)
                    current = Expression.MakeBinary(op, left, right);
                else
                    current = Expression.MakeUnary(op, right, null);
            }

            return current;
        }

        internal Expression ParseNumber(string expression, ref int i)
        {
            var raw = ReadTo(expression, Char.IsNumber, ref i);
            var value = Int32.Parse(raw);

            return Expression.Constant(value);
        }

        internal Expression ParseDate(string expression, ref int i)
        {
            i++;
            var raw = Unwrap(ReadTo(expression, '#', ref i), '#');
            var value = DateTime.Parse(raw);

            return Expression.Constant(value);
        }

        internal Expression ParseString(string expression, ref int i)
        {
            i++;
            var raw = Unwrap(ReadTo(expression, '\'', ref i), '\'');

            return Expression.Constant(raw);
        }

        internal string ReadTo(string expression, char test, ref int i)
        {
            return ReadTo(expression, (c) => c != test, ref i);
        }

        internal string ReadTo(string expression, Func<Char, bool> test, ref int i)
        {
            var start = i;
            for (; i < expression.Length && test(expression[i]); i++) ;
            return new String(expression.ToCharArray(), start, i - start);
        }

        internal string Unwrap(string wrapped, params char[] wraps)
        {
            if (wraps.Contains(wrapped[0]))
                wrapped = wrapped.Remove(0, 1);

            if (wraps.Contains(wrapped[wrapped.Length -1]))
                wrapped = wrapped.Remove(wrapped.Length - 1, 1);

            return wrapped;
        }

        internal Expression ParseName(string expression, ref int i)
        {
            return CreateGetter(Unwrap(ReadTo(expression, ']', ref i), '[', ']'));
        }

        internal Expression CreateGetter(string name)
        {
            if (typeof(TArg)==typeof(object))
                return CreateDynamicGetter(name);

            return Expression.Property(
                m_root, m_root.Type.GetProperty(name));
        }

        internal Expression CreateDynamicGetter(string name)
        {
            var type = typeof(object);
            var propBinder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                CSharpBinderFlags.None, name, 
                m_root.Type, 
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            return Expression.Dynamic(propBinder, type, new []{m_root});
        }
            
            
        internal ExpressionType ParseOperator(string expression, ref int i)
        {
            var raw = ReadTo(expression, Operators.Contains, ref i);
            i--;
            return OperatorMap[raw];
        }
    }
}