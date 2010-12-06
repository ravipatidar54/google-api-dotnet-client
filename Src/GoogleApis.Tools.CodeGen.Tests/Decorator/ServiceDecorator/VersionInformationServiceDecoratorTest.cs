/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.CodeDom;

using NUnit.Framework;

using Google.Apis.Discovery;
using Google.Apis.Testing;
using Google.Apis.Tools.CodeGen.Decorator.ServiceDecorator;

namespace Google.Apis.Tools.CodeGen.Tests.Decorator.ServiceDecorator
{
	[TestFixture()]
	public class VersionInformationServiceDecoratorTest:BaseServiceDecoratorTest
	{
		
		[Test()]
		public void TestCreateVersionField ()
		{
			var decorator = new VersionInformationServiceDecorator();
			var codeMemberField = decorator.CreateVersionField (CreateService ());
			
			Assert.AreEqual(VersionInformationServiceDecorator.VersionName, codeMemberField.Name);
			Assert.AreEqual(Version, ((CodePrimitiveExpression)codeMemberField.InitExpression).Value);
		}
		
		[Test()]
		public void TestCreateNameField ()
		{
			var decorator = new VersionInformationServiceDecorator();
			var codeMemberField = decorator.CreateNameField (CreateService ());
			
			Assert.AreEqual(VersionInformationServiceDecorator.NameName, codeMemberField.Name);
			Assert.AreEqual(Name, ((CodePrimitiveExpression)codeMemberField.InitExpression).Value);
		}
		
		[Test()]
		public void TestCreateUriField ()
		{
			var decorator = new VersionInformationServiceDecorator();
			var codeMemberField = decorator.CreateUriField (CreateService ());
			
			Assert.AreEqual(VersionInformationServiceDecorator.BaseUriName, codeMemberField.Name);
			Assert.AreEqual(BaseUri.ToString(), ((CodePrimitiveExpression)codeMemberField.InitExpression).Value);
		}
		
		[Test()]
		public void TestDecorateClass ()
		{
			var decorator = new VersionInformationServiceDecorator();
			CodeTypeDeclaration codeType = new CodeTypeDeclaration("TestClass");
			decorator.DecorateClass(CreateService(), codeType);
			
			Assert.AreEqual(3, codeType.Members.Count);
			
			CheckCompile(codeType, true, "Failed to compile VersionInformationServiceDecorator output");
		}
		
		
			
	}
}
