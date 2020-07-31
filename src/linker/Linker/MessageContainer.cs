﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using Mono.Cecil;

namespace Mono.Linker
{
	public readonly struct MessageContainer
	{
		public static readonly MessageContainer Empty = new MessageContainer ();

		/// <summary>
		/// Optional data with a filename, line and column that triggered the
		/// linker to output an error (or warning) message.
		/// </summary>
		public MessageOrigin? Origin { get; }

		public MessageCategory Category { get; }

		/// <summary>
		/// Further categorize the message.
		/// </summary>
		public string SubCategory { get; }

		/// <summary>
		/// Code identifier for errors and warnings reported by the IL linker.
		/// </summary>
		public int? Code { get; }

		/// <summary>
		/// Version number for this warning.
		public WarnVersion? Version { get; }

		/// <summary>
		/// User friendly text describing the error or warning.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Create an error message.
		/// </summary>
		/// <param name="text">Humanly readable message describing the error</param>
		/// <param name="code">Unique error ID. Please see https://github.com/mono/linker/blob/master/doc/error-codes.md
		/// for the list of errors and possibly add a new one</param>
		/// <param name="subcategory">Optionally, further categorize this error</param>
		/// <param name="origin">Filename, line, and column where the error was found</param>
		/// <returns>New MessageContainer of 'Error' category</returns>
		public static MessageContainer CreateErrorMessage (string text, int code, string subcategory = MessageSubCategory.None, MessageOrigin? origin = null, bool isWarnAsError = false, WarnVersion? version = null)
		{
			if (isWarnAsError) {
				ValidateWarning (code, version);
			} else {
				if (!(code >= 1000 && code <= 2000))
					throw new ArgumentException ($"The provided code '{code}' does not fall into the error category, which is in the range of 1000 to 2000 (inclusive).");
				if (version != null)
					throw new ArgumentException ($"An error message may not have a version.");
			}

			return new MessageContainer (MessageCategory.Error, text, code, subcategory, origin, version);
		}

		/// <summary>
		/// Create a warning message.
		/// </summary>
		/// <param name="context">Context with the relevant warning suppression info.</param>
		/// <param name="text">Humanly readable message describing the warning</param>
		/// <param name="code">Unique warning ID. Please see https://github.com/mono/linker/blob/master/doc/error-codes.md
		/// for the list of warnings and possibly add a new one</param>
		/// /// <param name="origin">Filename or member where the warning is coming from</param>
		/// <param name="subcategory">Optionally, further categorize this warning</param>
		/// <param name="version">Optional warning version number. Versioned warnings can be controlled with the
		/// warning wave option --warn VERSION. Unversioned warnings are unaffected by this option. </param>
		/// <returns>New MessageContainer of 'Warning' category</returns>
		public static MessageContainer CreateWarningMessage (LinkContext context, string text, int code, MessageOrigin origin, string subcategory = MessageSubCategory.None, WarnVersion? version = null)
		{
			ValidateWarning (code, version);

			if (context.IsWarningSuppressed (code, origin))
				return Empty;

			return new MessageContainer (MessageCategory.Warning, text, code, subcategory, origin, version);
		}

		/// <summary>
		/// Create a info message.
		/// </summary>
		/// <param name="text">Humanly readable message</param>
		/// <returns>New MessageContainer of 'Info' category</returns>
		public static MessageContainer CreateInfoMessage (string text)
		{
			return new MessageContainer (MessageCategory.Info, text, null);
		}

		/// <summary>
		/// Create a diagnostics message.
		/// </summary>
		/// <param name="text">Humanly readable message</param>
		/// <returns>New MessageContainer of 'Diagnostic' category</returns>
		public static MessageContainer CreateDiagnosticMessage (string text)
		{
			return new MessageContainer (MessageCategory.Diagnostic, text, null);
		}

		private MessageContainer (MessageCategory category, string text, int? code, string subcategory = MessageSubCategory.None, MessageOrigin? origin = null, WarnVersion? version = null)
		{
			Code = code;
			Category = category;
			Origin = origin;
			SubCategory = subcategory;
			Text = text;
			Version = version;
		}

		static void ValidateWarning (int code, WarnVersion? version)
		{
			if (!(code > 2000 && code <= 6000))
				throw new ArgumentException ($"The provided code '{code}' does not fall into the warning category, which is in the range of 2001 to 6000 (inclusive).");

			if (version != null && !(version >= WarnVersion.ILLink0 && version <= WarnVersion.Latest))
				throw new ArgumentException ($"The provided warning version '{version}' is invalid.");
		}

		public override string ToString () => ToMSBuildString ();

		public string ToMSBuildString ()
		{
			const string originApp = Constants.ILLink;
			string origin = Origin?.ToString () ?? originApp;

			StringBuilder sb = new StringBuilder ();
			sb.Append (origin).Append (":");

			if (!string.IsNullOrEmpty (SubCategory))
				sb.Append (" ").Append (SubCategory);

			string cat;
			switch (Category) {
			case MessageCategory.Error:
				cat = "error";
				break;
			case MessageCategory.Warning:
				cat = "warning";
				break;
			default:
				cat = "";
				break;
			}

			if (!string.IsNullOrEmpty (cat)) {
				sb.Append (" ")
					.Append (cat)
					.Append (" IL")
					.Append (Code.Value.ToString ("D4"))
					.Append (": ");
			} else {
				sb.Append (" ");
			}

			if (Origin?.MemberDefinition != null) {
				if (Origin?.MemberDefinition is MethodDefinition method)
					sb.Append (method.GetDisplayName ());
				else
					sb.Append (Origin?.MemberDefinition.FullName);

				sb.Append (": ");
			}

			// Expected output $"{FileName(SourceLine, SourceColumn)}: {SubCategory}{Category} IL{Code}: ({MemberDisplayName}: ){Text}");
			sb.Append (Text);
			return sb.ToString ();
		}

		public bool Equals (MessageContainer other) =>
			(Category, Text, Code, SubCategory, Origin) == (other.Category, other.Text, other.Code, other.SubCategory, other.Origin);

		public override bool Equals (object obj) => obj is MessageContainer messageContainer && Equals (messageContainer);
		public override int GetHashCode () => (Category, Text, Code, SubCategory, Origin).GetHashCode ();
		public static bool operator == (MessageContainer lhs, MessageContainer rhs) => lhs.Equals (rhs);
		public static bool operator != (MessageContainer lhs, MessageContainer rhs) => !lhs.Equals (rhs);
	}
}
