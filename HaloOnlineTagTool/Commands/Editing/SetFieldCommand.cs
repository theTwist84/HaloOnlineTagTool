﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HaloOnlineTagTool.Serialization;
using HaloOnlineTagTool.TagStructures;
using HaloOnlineTagTool.Common;

namespace HaloOnlineTagTool.Commands.Editing
{
    class SetFieldCommand : Command
    {
        public OpenTagCache Info { get; }

        public HaloTag Tag { get; }

        public TagStructureInfo Structure { get; }

        public object Owner { get; }

        public SetFieldCommand(OpenTagCache info, HaloTag tag, TagStructureInfo structure, object value)
            : base(CommandFlags.Inherit,
                  "SetField",
                  $"Sets the value of a specific field in the current {structure.Types[0].Name} definition.",
                  "SetField <field name> <field value>",
                  $"Sets the value of a specific field in the current {structure.Types[0].Name} definition.")
        {
            Info = info;
            Tag = tag;
            Structure = structure;
            Owner = value;
        }

        public override bool Execute(List<string> args)
        {
            if (args.Count < 2)
                return false;

            var enumerator = new TagFieldEnumerator(Structure);

            var field = enumerator.Find(f => f.Name == args[0]);

            if (field == null)
            {
                Console.WriteLine("ERROR: {0} does not contain a field named \"{1}\".", Owner.GetType().Name, args[1]);
                return false;
            }

            var value = ParseArgs(field.FieldType, args.Skip(1).ToList());

            if (value.Equals(false))
                return false;

            field.SetValue(Owner, value);
            
            Console.WriteLine("{0}: {1} = {2}", field.Name, field.FieldType.Name, field.GetValue(Owner));

            return true;
        }

        public object ParseArgs(Type type, List<string> args)
        {
            var input = args[0];
            object output = null;

            if (type.IsArray)
                throw new NotImplementedException();

            if (type == typeof(byte))
            {
                if (args.Count != 1)
                    return false;
                byte value;
                if (!byte.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(sbyte))
            {
                if (args.Count != 1)
                    return false;
                sbyte value;
                if (!sbyte.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(short))
            {
                if (args.Count != 1)
                    return false;
                short value;
                if (!short.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(ushort))
            {
                if (args.Count != 1)
                    return false;
                ushort value;
                if (!ushort.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(int))
            {
                if (args.Count != 1)
                    return false;
                int value;
                if (!int.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(uint))
            {
                if (args.Count != 1)
                    return false;
                uint value;
                if (!uint.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(long))
            {
                if (args.Count != 1)
                    return false;
                long value;
                if (!long.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(ulong))
            {
                if (args.Count != 1)
                    return false;
                ulong value;
                if (!ulong.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(float))
            {
                if (args.Count != 1)
                    return false;
                float value;
                if (!float.TryParse(input, out value))
                    return false;
                output = value;
            }
            else if (type == typeof(HaloTag))
            {
                if (args.Count != 1)
                    return false;
                output = ArgumentParser.ParseTagIndex(Info.Cache, input);
            }
            else if (type == typeof(StringId))
            {
                if (args.Count != 1)
                    return false;
                output = Info.StringIds.GetStringId(input);
            }
            else if (type == typeof(Angle))
            {
                if (args.Count != 1)
                    return false;
                float value;
                if (!float.TryParse(input, out value))
                    return false;
                output = Angle.FromDegrees(value);
            }
            else if (type == typeof(Vector2))
            {
                if (args.Count != 2)
                    return false;
                float x, y;
                if (!float.TryParse(args[0], out x) ||
                    !float.TryParse(args[1], out y))
                    return false;
                output = new Vector2(x, y);
            }
            else if (type == typeof(Vector3))
            {
                if (args.Count != 3)
                    return false;
                float x, y, z;
                if (!float.TryParse(args[0], out x) ||
                    !float.TryParse(args[1], out y) ||
                    !float.TryParse(args[2], out z))
                    return false;
                output = new Vector3(x, y, z);
            }
            else if (type == typeof(Vector4))
            {
                if (args.Count != 4)
                    return false;
                float x, y, z, w;
                if (!float.TryParse(args[0], out x) ||
                    !float.TryParse(args[1], out y) ||
                    !float.TryParse(args[2], out z) ||
                    !float.TryParse(args[3], out w))
                    return false;
                output = new Vector4(x, y, z, w);
            }
            else if (type.IsEnum)
            {
                if (args.Count != 1)
                    return false;

                var names = Enum.GetNames(type).ToList();
                var found = names.Find(n => n == args[0]);

                if (found == null)
                {
                    Console.WriteLine("Invalid {0} enum option: {1}", type.Name, args[0]);
                    Console.WriteLine("");

                    Console.WriteLine("Valid options:");
                    foreach (var name in names)
                        Console.WriteLine("\t{0}", name);
                    Console.WriteLine();

                    return false;
                }

                output = Enum.Parse(type, args[0]);
            }
            else if (type == typeof(Range<>))
            {
                var rangeType = type.GenericTypeArguments[0];
                var argCount = RangeArgCount(rangeType);
                
                var min = ParseArgs(rangeType, args.Take(argCount).ToList());

                if (min.Equals(false))
                    return false;

                var max = ParseArgs(rangeType, args.Skip(argCount).Take(argCount).ToList());

                if (max.Equals(false))
                    return false;

                output = Activator.CreateInstance(type, new object[] { min, max });
            }
            else throw new NotImplementedException();

            return output;
        }

        private int RangeArgCount(Type type)
        {
            if (type.IsEnum ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(HaloTag) ||
                type == typeof(StringId) ||
                type == typeof(Angle))
                return 1;
            else if (type == typeof(Vector2))
                return 2;
            else if (type == typeof(Vector3))
                return 3;
            else if (type == typeof(Vector4))
                return 4;
            else throw new NotImplementedException();
        }
    }
}
