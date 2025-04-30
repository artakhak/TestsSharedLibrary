// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OROptimizer.Diagnostics;
using OROptimizer.Diagnostics.Log;
using OROptimizer.Utilities;
using TestsSharedLibrary.Diagnostics.Log;

namespace TestsSharedLibrary;

public static class TestsHelper
{
    public delegate void ObjectsEqualityValidationDelegate(object expectedObject, object actualObject);
    
    /// <summary>
    /// Represents the maximum number of calls allowed to the validation method
    /// to prevent potential infinite loops during object equality validation.
    /// <see cref="ValidateObjectsAreEqualAsync" /> to detect a situation when we get in infinite loop.
    /// </summary>
    private const int MaxCallCountForValidateObjectsAreEqual = 10000000;

    /// <summary>
    ///     Executes the task returned by <paramref name="getTask" /> and cancels after
    ///     <paramref name="millisecondsAfterWhichToCancel" />
    /// </summary>
    /// <param name="getTask">A function that returns a task to execute.</param>
    /// <param name="millisecondsAfterWhichToCancel">Maximum milliseconds to allow for the task execution</param>
    /// <param name="taskName">Task name.</param>
    /// <param name="getIdentifier">A function that returns identifier to identify the state when the task was cancelled.</param>
    /// <exception cref="OperationCanceledException">Throws this exception.</exception>
    /// <exception cref="Exception">Throws this exception if execution of <paramref name="getTask" /> throws an exception.</exception>
    public static async Task ExecuteTaskWithCancellationAsync([NotNull] Func<Task> getTask, int millisecondsAfterWhichToCancel, [NotNull] string taskName, [CanBeNull] Func<string> getIdentifier = null)
    {
        var taskExecutionIsComplete = false;

        async Task CancellationTaskAsync()
        {
            var startTime = DateTime.Now;

            if (taskExecutionIsComplete)
                return;

            while (true)
            {
                await Task.Delay(500).ConfigureAwait(false);

                if (taskExecutionIsComplete)
                    return;

                if (DateTime.Now > startTime.AddMilliseconds(millisecondsAfterWhichToCancel))
                    throw new Exception($"Task {taskName} was canceled after {millisecondsAfterWhichToCancel} milliseconds. {(getIdentifier == null ? "" : $"Id={getIdentifier()}")}.");
                //throw new Exception($"Task {taskName} was canceled after {millisecondsAfterWhichToCancel} milliseconds.  {nameof(ExpressionParser)}.{nameof(ExpressionParser.Counter)}={ExpressionParser.Counter}.");
            }
        }

        async Task TaskToExecute()
        {
            try
            {
                await getTask().ConfigureAwait(false);
            }
            finally
            {
                taskExecutionIsComplete = true;
            }
        }

        var completedTask = await Task.WhenAny(CancellationTaskAsync(), TaskToExecute()).ConfigureAwait(false);
        await completedTask.ConfigureAwait(false);
    }

    /// <summary>
    ///     Sets ups the logger and sets the log level to <see cref="LogLevel.Error" />.
    ///     Also, resets the test statistics
    /// </summary>
    public static void SetupLogger([CanBeNull] Log4TestsParameters log4TestsParameters = null)
    {
        LogHelper.RemoveContext();
        LogHelper.RegisterContext(new LogHelper4TestsContext(log4TestsParameters));
        Log4Tests.LogLevel = LogLevel.Info;
        Log4Tests.ResetLogStatistics();
    }

    public static void ValidateExceptionIsThrown([NotNull] Action action, [CanBeNull] Type exceptionType = null)
    {
        if (exceptionType == null)
            exceptionType = typeof(Exception);

        var loggedError = new StringBuilder();
        loggedError.AppendLine();
        loggedError.AppendLine("---------------------------------------------------------------");

        try
        {
            action();
        }
        catch (Exception e)
        {
            loggedError.AppendLine($"Validation failed. Exception message: {e.Message}");
            LogHelper.Context.Log.Error(loggedError.ToString());

            LogHelper.Context.Log.ErrorFormat("Exception stack trace: {0}", e.StackTrace);

            if (!exceptionType.IsInstanceOfType(e))
                throw new Exception(
                    $"The thrown exception should be of type '{exceptionType.FullName}'. The actual type of thrown exception is '{e.GetType().FullName}'.");

            return;
        }

        throw new Exception($"Expected exception of type '{exceptionType.FullName}' is expected.");
    }

    public static (bool isSuccess, string absoluteFilePath, string errorMessage) TryGetFilePathRelativeToTestProjectFolder([NotNull] string testProjectName,
        [NotNull] Type typeInTestProject, [NotNull] string fileFolderPathRelativeToTestProjectFolder)
    {
        return TryGetFilePathRelativeToTestProjectFolderLocal(testProjectName, typeInTestProject, fileFolderPathRelativeToTestProjectFolder);
    }

    public static (bool isSuccess, string absoluteFilePath, string errorMessage) TryGetTestProjectFolder([NotNull] string testProjectName,
        [NotNull] Type typeInTestProject)
    {
        return TryGetFilePathRelativeToTestProjectFolderLocal(testProjectName, typeInTestProject);
    }

    private static (bool isSuccess, string absoluteFilePath, string errorMessage) TryGetFilePathRelativeToTestProjectFolderLocal(
        [NotNull] string testProjectName,
        [NotNull] Type typeInTestProject,
        [CanBeNull] string fileFolderPathRelativeToTestProjectFolder = null)
    {
        var testAssemblyFilePath = Path.GetDirectoryName(typeInTestProject.Assembly.Location);

        var filePathSection = $@"\{testProjectName}\bin\";

        var indexOfTestProject = testAssemblyFilePath.IndexOf(filePathSection);

        if (indexOfTestProject < 0)
            return (false, null, $"The assembly location that owns type '{typeInTestProject}' does not contain '{filePathSection}'.");

        var testProjectAbsolutePath = testAssemblyFilePath.Substring(0, indexOfTestProject + 1 + testProjectName.Length);

        if (fileFolderPathRelativeToTestProjectFolder == null)
            return (true, testProjectAbsolutePath, null);

        return FilePathHelpers.TryGetAbsoluteFilePath(testProjectAbsolutePath, fileFolderPathRelativeToTestProjectFolder);
    }

    public static async Task ValidateExceptionIsThrownAsync([NotNull] Func<Task> getFailedTask, [CanBeNull] Type exceptionType = null)
    {
        if (exceptionType == null)
            exceptionType = typeof(Exception);

        try
        {
            await getFailedTask().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            LogHelper.Context.Log.Error("Validation failed.", e);

            if (!exceptionType.IsInstanceOfType(e))
                throw new Exception(
                    $"The thrown exception should be of type '{exceptionType.FullName}'. The actual type of thrown exception is '{e.GetType().FullName}'.");

            return;
        }

        throw new Exception($"Expected exception of type '{exceptionType.FullName}' is expected.");
    }

    //private class A
    //{
    //    public List<List<int>> Items = new List<List<int>>();
    //}

    public static Task ValidateObjectsAreEqualAsync(object expectedObject, object actualObject,
        int maxExecutionTimeInMilliseconds,
        [CanBeNull] Func<string> getIdentifier = null,
        [CanBeNull] Func<MemberInfo, bool> propertyValidationShouldBeIgnored = null,
        [CanBeNull] ObjectsEqualityValidationDelegate onObjectValidationFailed = null,
        [CanBeNull] Action<ValidatedObjectData> onObjectMemberValidationStarting = null,
        [CanBeNull] Action<ValidatedObjectData> objectMemberValidationCompleted = null)
    {
        //var a1 = new A();
        //var a2 = new A();

        //a1.Items.Add(new List<int>() { 1, 2, 3 });
        //a1.Items.Add(new List<int>() { 4, 5, 6 });

        //a2.Items.Add(new List<int>() { 1, 2, 3 });
        //a2.Items.Add(new List<int>() { 4, 5, 6 });

        //await ValidateObjectsAreEqualAsync(a1, a2).ConfigureAwait(false);

        //var temp = ValidateObjectsAreEqualAsync()
        //return ValidateObjectsAreEqualAsync(expectedObject, actualObject,
        //    propertyValidationShouldBeIgnored, onObjectValidationFailed);
        return ExecuteTaskWithCancellationAsync(() => ValidateObjectsAreEqualAsync(expectedObject, actualObject,
                propertyValidationShouldBeIgnored, onObjectValidationFailed, onObjectMemberValidationStarting, objectMemberValidationCompleted),
            maxExecutionTimeInMilliseconds, nameof(ValidateObjectsAreEqualAsync), getIdentifier);
    }

    private static bool IsPrimitiveType([NotNull] Type type)
    {
        return type == typeof(string) || !type.IsClass || type.IsPrimitive;
    }

    private static Task ValidateObjectsAreEqualAsync([CanBeNull] object expectedObjectParam, [CanBeNull] object actualObjectParam,
        [CanBeNull] Func<MemberInfo, bool> memberValidationShouldBeIgnored = null,
        [CanBeNull] ObjectsEqualityValidationDelegate onObjectValidationFailed = null,
        [CanBeNull] Action<ValidatedObjectData> onObjectMemberValidationStarting = null,
        [CanBeNull] Action<ValidatedObjectData> objectMemberValidationCompleted = null)
    {
        if (ReferenceEquals(expectedObjectParam, actualObjectParam))
            return Task.CompletedTask;

        if (expectedObjectParam == null != (actualObjectParam == null))
        {
            onObjectValidationFailed?.Invoke(expectedObjectParam, actualObjectParam);

            string nonNullParameterName;
            string nullParameterName;

            if (expectedObjectParam != null)
            {
                nonNullParameterName = nameof(expectedObjectParam);
                nullParameterName = nameof(actualObjectParam);
            }
            else
            {
                nonNullParameterName = nameof(actualObjectParam);
                nullParameterName = nameof(expectedObjectParam);
            }

            throw new Exception(
                $"The value of '{nonNullParameterName}' is not null while the value of '{nullParameterName}' is null.");
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (expectedObjectParam == null)
            return Task.CompletedTask;

        if (expectedObjectParam.GetType() != actualObjectParam.GetType())
            throw new Exception($"Expect type '{expectedObjectParam.GetType().FullName}' is different from actual type '{actualObjectParam.GetType().FullName}' are different.");

        bool MemberValidationShouldBeIgnored(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (memberValidationShouldBeIgnored?.Invoke(memberInfo) ?? false)
                return true;

            if (memberInfo.DeclaringType?.FullName?.StartsWith("System.Collections.Generic.List`1[") ?? false)
                if (memberInfo.Name == nameof(List<int>.Capacity) || memberInfo.Name == "Item")
                    return true;

            return false;
        }

        if (onObjectValidationFailed == null)
            onObjectValidationFailed = (_, _) => { };

        var objectsCache = new ObjectsCacheDefault();
        var comparedMemberKeys = new HashSet<string>();

        var validateObjectsAreEqualDataStack = new Stack<ObjectEqualityValidationData>();

        var rootValidateObjectsAreEqualData = new ObjectEqualityValidationData(objectsCache, comparedMemberKeys, null, expectedObjectParam, actualObjectParam);
        validateObjectsAreEqualDataStack.Push(rootValidateObjectsAreEqualData);

        var expectedObjectToComparedObjects = new Dictionary<object, HashSet<object>>();

        //int TESTCOUNTER = 0;
        while (validateObjectsAreEqualDataStack.Count > 0)
        {
            //LogHelper.Context.Log.Info($"TESTCOUNTER={TESTCOUNTER++}");

            if (validateObjectsAreEqualDataStack.Count > MaxCallCountForValidateObjectsAreEqual)
                throw new Exception($"Stack size is greater than {MaxCallCountForValidateObjectsAreEqual}");

            var stackItem = validateObjectsAreEqualDataStack.Peek();

            ValidatedObjectData validatedObjectData;

            if (stackItem.MoveNext(MemberValidationShouldBeIgnored, onObjectValidationFailed))
            {
                validatedObjectData = stackItem.GetCurrentData();

                if (validatedObjectData == null)
                    throw new Exception($"The value of {nameof(validatedObjectData)} cannot be null in this context.");

                onObjectMemberValidationStarting?.Invoke(validatedObjectData);
            }
            else
            {
                validateObjectsAreEqualDataStack.Pop();
                continue;
            }

            var memberInfo = validatedObjectData.MemberInfo;

            var expectedMemberValue = validatedObjectData.ExpectedObjectMemberValue;
            var actualMemberValue = validatedObjectData.ActualObjectMemberValue;

            try
            {
                if (expectedMemberValue == null != (actualMemberValue == null))
                    throw new Exception(
                        $"Error validating member {stackItem.GetCurrentMemberPath()}. Expected and actual values should be both null or both should not be null.");

                if (expectedMemberValue == null)
                    continue;

                if (ReferenceEquals(expectedMemberValue, actualMemberValue))
                    continue;

                var expectedObjectType = expectedMemberValue.GetType();

                if (IsPrimitiveType(expectedObjectType))
                {
                    if (!expectedMemberValue.Equals(actualMemberValue))
                    {
                        onObjectValidationFailed(expectedMemberValue, actualMemberValue);

                        var errorMessage = new StringBuilder();

                        errorMessage.Append($"Error validating member {stackItem.GetCurrentMemberPath()}. Expected and actual values are different for member '{memberInfo.Name}'.");

                        var expectedObjectForDisplay = expectedMemberValue;
                        var actualObjectForDisplay = actualMemberValue;

                        var stringMaxDisplayLength = 200;

                        if (expectedMemberValue is string expectedObjectToString && expectedObjectToString.Length > stringMaxDisplayLength)
                            expectedObjectForDisplay = $"{expectedObjectToString.Substring(0, stringMaxDisplayLength)} ...";

                        if (actualMemberValue is string actualObjectToString && actualObjectToString.Length > stringMaxDisplayLength)
                            actualObjectForDisplay = $"{actualObjectToString.Substring(0, stringMaxDisplayLength)} ...";

                        errorMessage.AppendLine("Expected value is:");
                        errorMessage.AppendLine($"'{expectedObjectForDisplay}'");

                        errorMessage.AppendLine($"Actual value is:");
                        errorMessage.AppendLine($"'{actualObjectForDisplay}'");

                        throw new Exception(errorMessage.ToString());
                    }
                }
                else
                {
                    if (!expectedObjectToComparedObjects.TryGetValue(expectedMemberValue, out var comparedObjectsSet))
                    {
                        comparedObjectsSet = new HashSet<object>();
                        expectedObjectToComparedObjects[expectedMemberValue] = comparedObjectsSet;
                    }
                    else if (comparedObjectsSet.Contains(actualMemberValue))
                    {
                        continue;
                    }

                    comparedObjectsSet.Add(actualMemberValue);

                    var memberValidationData = new ObjectEqualityValidationData(objectsCache, comparedMemberKeys, stackItem, expectedMemberValue, actualMemberValue);

                    validateObjectsAreEqualDataStack.Push(memberValidationData);
                }
            }
            finally
            {
                objectMemberValidationCompleted?.Invoke(validatedObjectData);
            }
        }

        if (validateObjectsAreEqualDataStack.Count != 0)
            throw new Exception("Stack size should be 0.");

        return Task.CompletedTask;
    }

    public class ValidatedObjectData
    {
        internal ValidatedObjectData([NotNull] ObjectInfo expectedParentObjectInfo, [NotNull] ObjectInfo actualParentObjectInfo, [NotNull] MemberInfo memberInfo, [CanBeNull] object expectedObjectMemberValue, [CanBeNull] object actualObjectMemberValue)
        {
            ExpectedParentObjectInfo = expectedParentObjectInfo;
            ActualParentObjectInfo = actualParentObjectInfo;
            MemberInfo = memberInfo;
            ExpectedObjectMemberValue = expectedObjectMemberValue;
            ActualObjectMemberValue = actualObjectMemberValue;
        }

        [NotNull] public ObjectInfo ExpectedParentObjectInfo { get; }

        [NotNull] public ObjectInfo ActualParentObjectInfo { get; }

        [NotNull] public MemberInfo MemberInfo { get; }

        [CanBeNull] public object ExpectedObjectMemberValue { get; }

        [CanBeNull] public object ActualObjectMemberValue { get; }
    }

    private class ObjectEqualityValidationData
    {
        private readonly HashSet<string> _comparedMemberKeys;

        [CanBeNull] private readonly MethodInfo _getEnumeratorMethodInfo;

        [NotNull] private readonly ObjectsCacheDefault _objectsCache;

        [CanBeNull] private readonly ObjectEqualityValidationData _parentObjectEqualityValidationData;

        [NotNull] private readonly IEnumerator<MemberInfo> _typeMembersEnumerator;

        [CanBeNull] private object _actualObjectMemberValue;

        [CanBeNull] private object[] _currentActualMemberArray;


        [CanBeNull] private object[] _currentExpectedMemberArray;

        [CanBeNull] private object _expectedObjectMemberValue;

        private int _indexInCurrentMemberArray = -1;

        public ObjectEqualityValidationData([NotNull] ObjectsCacheDefault objectsCache, HashSet<string> comparedMemberKeys,
            [CanBeNull] ObjectEqualityValidationData parentObjectEqualityValidationData,
            [NotNull] object expectedParentObject, [NotNull] object actualParentObject)
        {
            _objectsCache = objectsCache;
            _comparedMemberKeys = comparedMemberKeys;
            _parentObjectEqualityValidationData = parentObjectEqualityValidationData;
            ExpectedParentObjectInfo = objectsCache.GetOrCreateObjectInfo(expectedParentObject);
            ActualParentObjectInfo = objectsCache.GetOrCreateObjectInfo(actualParentObject);

            if (expectedParentObject.GetType() != actualParentObject.GetType())
                throw new Exception($"The types of expected and actual parent objects are not the same. The expected type is {expectedParentObject.GetType().FullName} and the actual type is {actualParentObject.GetType()}");

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var expectedObjectType = expectedParentObject.GetType();
            var typeMembers = new List<MemberInfo>(expectedObjectType.GetProperties(bindingFlags));
            typeMembers.AddRange(expectedObjectType.GetFields(bindingFlags));

            if (expectedParentObject is IEnumerable)
            {
                _getEnumeratorMethodInfo = expectedParentObject.GetType().GetMethod(nameof(IEnumerable.GetEnumerator));

                //var expectedArray = expectedParentObject as object[] ?? enumerable.Cast<object>().ToArray();
                //var actualArray = actualParentObject as object[] ?? ((IEnumerable)actualParentObject).Cast<object>().ToArray();
                //Assert.AreEqual(expectedArray.Length, actualArray.Length);

                typeMembers.Add(_getEnumeratorMethodInfo);
            }

            _typeMembersEnumerator = typeMembers.GetEnumerator();
        }

        [NotNull] public ObjectInfo ExpectedParentObjectInfo { get; }

        [NotNull] public ObjectInfo ActualParentObjectInfo { get; }

        [CanBeNull]
        public ValidatedObjectData GetCurrentData()
        {
            if (_typeMembersEnumerator.Current != null)
            {
                var typeMemberInfo = _typeMembersEnumerator.Current;

                if (_indexInCurrentMemberArray >= 0)
                    // ReSharper disable once PossibleNullReferenceException
                    return new ValidatedObjectData(ExpectedParentObjectInfo, ActualParentObjectInfo,
                        typeMemberInfo, _currentExpectedMemberArray[_indexInCurrentMemberArray],
                        // ReSharper disable once PossibleNullReferenceException
                        _currentActualMemberArray[_indexInCurrentMemberArray]);

                return new ValidatedObjectData(ExpectedParentObjectInfo, ActualParentObjectInfo, typeMemberInfo, _expectedObjectMemberValue, _actualObjectMemberValue);
            }

            return null;
        }

        public string GetCurrentMemberPath()
        {
            var membersPath = new LinkedList<ObjectEqualityValidationData>();

            var currentParentNode = _parentObjectEqualityValidationData;
            var rootObjectData = this;

            while (currentParentNode != null)
            {
                rootObjectData = currentParentNode;
                membersPath.AddFirst(currentParentNode);
                currentParentNode = currentParentNode._parentObjectEqualityValidationData;
            }

            var memberPathToText = new StringBuilder();

            memberPathToText.Append(rootObjectData.ExpectedParentObjectInfo.Object.GetType().FullName);

            var currentPathItemNode = membersPath.First;

            while (currentPathItemNode != null)
            {
                var currentPathItem = currentPathItemNode.Value;

                if (currentPathItem._typeMembersEnumerator.Current == null)
                    throw new ArgumentNullException();

                if (_expectedObjectMemberValue != ExpectedParentObjectInfo.Object)
                    memberPathToText.Append($".{currentPathItem._typeMembersEnumerator.Current.Name}");

                if (currentPathItem._currentExpectedMemberArray != null)
                {
                    if (_indexInCurrentMemberArray < currentPathItem._currentExpectedMemberArray.Length)
                    {
                        memberPathToText.Append($"[{_indexInCurrentMemberArray}]");
                    }
                    else
                    {
                        if (!(_indexInCurrentMemberArray == 0 &&
                              currentPathItem._currentExpectedMemberArray.Length == 0))
                            throw new Exception($"Error in {nameof(GetCurrentMemberPath)}.");
                        memberPathToText.Append($"[{_indexInCurrentMemberArray}]");
                    }
                }

                currentPathItemNode = currentPathItemNode.Next;
            }

            return memberPathToText.ToString();
        }

        public bool MoveNext([CanBeNull] Func<MemberInfo, bool> propertyValidationShouldBeIgnored = null, [CanBeNull] ObjectsEqualityValidationDelegate onObjectValidationFailed = null)
        {
            if (_currentExpectedMemberArray != null && ++_indexInCurrentMemberArray < _currentExpectedMemberArray.Length)
                return true;

            _indexInCurrentMemberArray = -1;
            _currentExpectedMemberArray = null;
            _currentActualMemberArray = null;

            while (_typeMembersEnumerator.MoveNext())
            {
                if (_typeMembersEnumerator.Current == null)
                    throw new ArgumentException();

                var typeMemberInfo = _typeMembersEnumerator.Current;

                if (propertyValidationShouldBeIgnored?.Invoke(typeMemberInfo) ?? false)
                    continue;

                if (typeMemberInfo != _getEnumeratorMethodInfo)
                {
                    switch (_typeMembersEnumerator.Current.MemberType)
                    {
                        case MemberTypes.Property:

                            var propertyInfo = (PropertyInfo) _typeMembersEnumerator.Current;
                            _expectedObjectMemberValue = propertyInfo.GetValue(ExpectedParentObjectInfo.Object);
                            _actualObjectMemberValue = propertyInfo.GetValue(ActualParentObjectInfo.Object);
                            break;

                        case MemberTypes.Field:

                            var fieldInfo = (FieldInfo) _typeMembersEnumerator.Current;
                            _expectedObjectMemberValue = fieldInfo.GetValue(ExpectedParentObjectInfo.Object);
                            _actualObjectMemberValue = fieldInfo.GetValue(ActualParentObjectInfo.Object);
                            break;
                    }
                }
                else
                {
                    _expectedObjectMemberValue = ExpectedParentObjectInfo.Object;
                    _actualObjectMemberValue = ActualParentObjectInfo.Object;
                }

                if (_expectedObjectMemberValue == null || _actualObjectMemberValue == null)
                {
                    if (_expectedObjectMemberValue == null != (_actualObjectMemberValue == null))
                    {
                        onObjectValidationFailed?.Invoke(_expectedObjectMemberValue, _actualObjectMemberValue);

                        string nonNullObjectName;
                        string nullObjectName;

                        if (_expectedObjectMemberValue != null)
                        {
                            nonNullObjectName = "expected object";
                            nullObjectName = "actual object";
                        }
                        else
                        {
                            nonNullObjectName = "actual object";
                            nullObjectName = "expected object";
                        }

                        throw new Exception($"Error validating member {GetCurrentMemberPath()}. The value of member {typeMemberInfo.Name} in '{nonNullObjectName}' is not null while this value is null in '{nullObjectName}'.");
                    }

                    return true;
                }

                if (_actualObjectMemberValue == null)
                    throw new Exception($"Error validating member {GetCurrentMemberPath()}. The value of {_actualObjectMemberValue} is null.");

                var expectedObjectMemberValueType = _expectedObjectMemberValue.GetType();

                if (expectedObjectMemberValueType != _actualObjectMemberValue.GetType())
                    // ReSharper disable once PossibleNullReferenceException
                    throw new Exception($"Error validating member {GetCurrentMemberPath()}. The types of {typeMemberInfo.Name} are different in expected and actual object. Expected object member type is '{expectedObjectMemberValueType.FullName}' and actual object member type is '{_actualObjectMemberValue.GetType().FullName}'.");

                if (!IsPrimitiveType(_expectedObjectMemberValue.GetType()))
                {
                    var expectedObjectMemberValueInfo = _objectsCache.GetOrCreateObjectInfo(_expectedObjectMemberValue);
                    var actualObjectMemberValueInfo = _objectsCache.GetOrCreateObjectInfo(_actualObjectMemberValue);

                    var comparisonKey = $"{expectedObjectMemberValueInfo.ObjectId}_{actualObjectMemberValueInfo.ObjectId}_{typeMemberInfo.Name}";

                    if (_comparedMemberKeys.Contains(comparisonKey))
                        continue;

                    _comparedMemberKeys.Add(comparisonKey);

                    if (_expectedObjectMemberValue is IEnumerable enumerable1)
                    {
                        var expectedArray = enumerable1 as object[] ?? enumerable1.Cast<object>().ToArray();
                        var actualArray = _actualObjectMemberValue as object[] ?? ((IEnumerable) _actualObjectMemberValue).Cast<object>().ToArray();

                        if (expectedArray.Length == actualArray.Length)
                        {
                            if (expectedArray.Length > 0)
                            {
                                _indexInCurrentMemberArray = 0;
                                _currentExpectedMemberArray = expectedArray;
                                _currentActualMemberArray = actualArray;
                            }
                            else
                            {
                                _expectedObjectMemberValue = null;
                                _actualObjectMemberValue = null;
                            }
                        }
                        else
                        {
                            throw new Exception($"Error validating member {GetCurrentMemberPath()}. Number of items in expected and actual member '{nameof(typeMemberInfo.Name)}' are different.");
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}