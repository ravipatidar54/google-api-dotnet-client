﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Util;

namespace Google.Apis.Tools.CodeGen.Decorator
{
    /// <summary>
    /// Helper class for all decorators
    /// </summary>
    internal static class DecoratorUtil
    {
        /// <summary>
        /// Creates and adds a public auto-property (property and backening field) to the class
        /// </summary>
        /// <typeparam name="TProperty">Type used for the propery</typeparam>
        /// <param name="serviceClass"></param>
        /// <param name="name"></param>
        /// <param name="summaryComment"></param>
        public static CodeMemberProperty AddAutoProperty<TProperty>(CodeTypeDeclaration serviceClass, 
                                                                    string name, string summaryComment)
        {
            // Validate parameters
            serviceClass.ThrowIfNull("serviceClass");
            name.ThrowIfNullOrEmpty("name");

            // Check if the name has already been used
            if (serviceClass.Members.FindPropertyByName(name) != null)
                throw new ArgumentException(
                    string.Format("The property name [{0}] was already used within this class", name),
                    "name");

            // Create backening field
            var field = CreateBackeningField<TProperty>(serviceClass, name);
            string fieldName = field.Name;
            var fieldNameRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);

            // Add property
            var property = new CodeMemberProperty();
            property.Name = name;
            property.Attributes = MemberAttributes.Public;

            if (summaryComment.IsNotNullOrEmpty())
            {
                property.Comments.Add(
                    new CodeCommentStatement(
                        "<summary>" + summaryComment + "</summary>", true));
            }
            property.Type = new CodeTypeReference(typeof(TProperty));
            property.HasGet = true;
            property.HasSet = true;

            // Add getter and setter
            property.GetStatements.Add(new CodeMethodReturnStatement(fieldNameRef));

            property.SetStatements.Add(
                new CodeAssignStatement(fieldNameRef,
                                        new CodePropertySetValueReferenceExpression()));

            // Return the result
            serviceClass.Members.Add(property);
            return property;
        }

        /// <summary>
        /// Creates a backening field for the name provided
        /// </summary>
        /// <typeparam name="TProperty">Type used for the propery</typeparam>
        /// <param name="serviceClass"></param>
        /// <param name="name">The name of the property</param>
        /// <returns></returns>
        public static CodeMemberField CreateBackeningField<TProperty>(CodeTypeDeclaration serviceClass, string name)
        {
            // Validate parameters
            serviceClass.ThrowIfNull("serviceClass");
            name.ThrowIfNullOrEmpty("name");

            // Generate field name
            var fieldName = Char.IsLower(name[0])
                                ? "_" + name
                                : GeneratorUtils.LowerFirstLetter(name);

            // Check if it was already used
            if (serviceClass.Members.FindFieldByName(fieldName) != null)
                throw new ArgumentException(
                    string.Format("The property name [{0}] was already used within this class", name),
                    "name");

            // Add the field
            var field = new CodeMemberField(typeof(TProperty), fieldName);
            field.Attributes = MemberAttributes.Private;
            serviceClass.Members.Add(field);

            // Return the field name
            return field;
        }
    }
}