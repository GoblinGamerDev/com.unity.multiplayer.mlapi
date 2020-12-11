using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MLAPI.Messaging;
using MLAPI.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using UnityEngine;

namespace MLAPI.Editor.CodeGen
{
    internal static class CodeGenHelpers
    {
        public const string RuntimeAssemblyName = "Unity.Multiplayer.MLAPI.Runtime";

        public static readonly string NetworkBehaviour_FullName = typeof(NetworkedBehaviour).FullName;
        public static readonly string ServerRPCAttribute_FullName = typeof(ServerRPCAttribute).FullName;
        public static readonly string ClientRPCAttribute_FullName = typeof(ClientRPCAttribute).FullName;
        public static readonly string ServerRpcParams_FullName = typeof(ServerRpcParams).FullName;
        public static readonly string ClientRpcParams_FullName = typeof(ClientRpcParams).FullName;
        public static readonly string INetworkSerializable_FullName = typeof(INetworkSerializable).FullName;
        public static readonly string INetworkSerializable_NetworkRead_Name = nameof(INetworkSerializable.NetworkRead);
        public static readonly string INetworkSerializable_NetworkWrite_Name = nameof(INetworkSerializable.NetworkWrite);
        public static readonly string IEnumerable_FullName = typeof(IEnumerable<>).FullName;
        public static readonly string UnityColor_FullName = typeof(Color).FullName;
        public static readonly string UnityVector2_FullName = typeof(Vector2).FullName;
        public static readonly string UnityVector3_FullName = typeof(Vector3).FullName;
        public static readonly string UnityVector4_FullName = typeof(Vector4).FullName;
        public static readonly string UnityQuaternion_FullName = typeof(Quaternion).FullName;
        public static readonly string UnityRay_FullName = typeof(Ray).FullName;
        public static readonly string UnityRay2D_FullName = typeof(Ray2D).FullName;

        public static uint Hash(this MethodDefinition methodDefinition)
        {
            var sigArr = Encoding.UTF8.GetBytes($"{methodDefinition.Module.Name} => {methodDefinition.FullName}");
            var sigLen = sigArr.Length;
            unsafe
            {
                fixed (byte* sigPtr = sigArr)
                {
                    return XXHash.Hash32(sigPtr, sigLen);
                }
            }
        }

        public static bool IsSubclassOf(this TypeDefinition typeDefinition, string ClassTypeFullName)
        {
            if (!typeDefinition.IsClass) return false;

            var baseTypeRef = typeDefinition.BaseType;
            while (baseTypeRef != null)
            {
                if (baseTypeRef.FullName == ClassTypeFullName)
                {
                    return true;
                }

                try
                {
                    baseTypeRef = baseTypeRef.Resolve().BaseType;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public static bool HasInterface(this TypeReference typeReference, string InterfaceTypeFullName)
        {
            try
            {
                var typeDef = typeReference.Resolve();
                var typeFaces = typeDef.Interfaces;
                return typeFaces.Any(iface => iface.InterfaceType.FullName == InterfaceTypeFullName);
            }
            catch
            {
            }

            return false;
        }

        public static bool HasGenericInterface(this TypeReference typeReference, string GenericInterfaceTypeFullName)
        {
            try
            {
                var typeDef = typeReference.Resolve();
                var typeFaces = typeDef.Interfaces;
                return typeFaces.Any(iface => iface.InterfaceType.FullName.StartsWith(GenericInterfaceTypeFullName));
            }
            catch
            {
            }

            return false;
        }

        public static bool IsSupportedType(this TypeReference typeReference)
        {
            var typeSystem = typeReference.Module.TypeSystem;

            // common primitives
            if (typeReference == typeSystem.Boolean) return true;
            if (typeReference == typeSystem.Char) return true;
            if (typeReference == typeSystem.SByte) return true;
            if (typeReference == typeSystem.Byte) return true;
            if (typeReference == typeSystem.Int16) return true;
            if (typeReference == typeSystem.UInt16) return true;
            if (typeReference == typeSystem.Int32) return true;
            if (typeReference == typeSystem.UInt32) return true;
            if (typeReference == typeSystem.Int64) return true;
            if (typeReference == typeSystem.UInt64) return true;
            if (typeReference == typeSystem.Single) return true;
            if (typeReference == typeSystem.Double) return true;
            if (typeReference == typeSystem.String) return true;

            // Unity primitives
            if (typeReference.FullName == UnityColor_FullName) return true;
            if (typeReference.FullName == UnityVector2_FullName) return true;
            if (typeReference.FullName == UnityVector3_FullName) return true;
            if (typeReference.FullName == UnityVector4_FullName) return true;
            if (typeReference.FullName == UnityQuaternion_FullName) return true;
            if (typeReference.FullName == UnityRay_FullName) return true;
            if (typeReference.FullName == UnityRay2D_FullName) return true;

            // INetworkSerializable
            if (typeReference.HasInterface(INetworkSerializable_FullName)) return true;

            // Enum
            if (typeReference.GetEnumAsInt() != null) return true;

            return false;
        }

        public static TypeReference GetEnumAsInt(this TypeReference typeReference)
        {
            try
            {
                var typeDef = typeReference.Resolve();
                if (typeDef.IsEnum)
                {
                    return typeDef.GetEnumUnderlyingType();
                }
            }
            catch
            {
            }

            return null;
        }

        public static void AddError(this List<DiagnosticMessage> diagnostics, string message)
        {
            diagnostics.AddError((SequencePoint)null, message);
        }

        public static void AddError(this List<DiagnosticMessage> diagnostics, MethodDefinition methodDefinition, string message)
        {
            diagnostics.AddError(methodDefinition.DebugInformation.SequencePoints[0], message);
        }

        public static void AddError(this List<DiagnosticMessage> diagnostics, SequencePoint sequencePoint, string message)
        {
            diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0,
                MessageData = message
            });
        }
    }
}