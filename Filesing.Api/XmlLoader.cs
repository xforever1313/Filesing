//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using SethCS.Extensions;

namespace Filesing.Api
{
    public static class XmlLoader
    {
        // ---------------- Fields ----------------

        private const string rootNodeName = "filesingConfig";

        private const string globalSettingsNode = "globalSettings";

        private const string ignoreListNode = "ignores";

        private const string ignoreFileNode = "ignoreFile";

        private const string ignoreFileWithRegexNode = "ignoreFileWithRegex";

        private const string ignoreFileWithExtensionNode = "ignoreFileWithExtension";

        private const string ignoreDirNode = "ignoreDir";

        private const string ignoreDirWithRegexNode = "ignoreDirWithRegex";

        private const string requirementsListNode = "requirements";

        private const string requireFileNode = "requireFile";

        private const string requireDirNode = "requireDir";

        private const string patternsListNode = "patterns";

        private const string patternNode = "pattern";

        private const string regexNode = "regex";

        private const string ignoreCaseAttr = "ignoreCase";

        // ---------------- Functions ----------------

        public static FilesingConfig LoadConfigFromXml( string xmlFilePath, string searchDir )
        {
            FilesingConfig config = new FilesingConfig
            {
                SearchDirectoryLocation = searchDir
            };

            XmlDocument doc = new XmlDocument();
            doc.Load( xmlFilePath );

            XmlNode rootNode = doc.DocumentElement;
            if( rootNode.Name.EqualsIgnoreCase( rootNodeName ) == false )
            {
                throw new ArgumentException(
                    "Root node is not '" + rootNodeName + "', is this the correct XML file?  Got: " + rootNode.Name
                );
            }

            foreach( XmlNode childNodes in rootNode.ChildNodes )
            {
                if( childNodes.Name.EqualsIgnoreCase( globalSettingsNode ) )
                {
                    ParseGlobalSettings( childNodes, config );
                }
                else if( childNodes.Name.EqualsIgnoreCase( patternsListNode ) )
                {
                    ParsePatterns( childNodes, config );
                }
            }

            return config;
        }

        private static void ParseGlobalSettings( XmlNode globalSettingsNode, FilesingConfig filesingConfig )
        {
            foreach( XmlNode childNode in globalSettingsNode.ChildNodes )
            {
                if( childNode.Name.EqualsIgnoreCase( ignoreListNode ) )
                {
                    filesingConfig.GlobalIgnoreConfigs.Add( LoadIgnoreConfig( childNode, filesingConfig ) );
                }
                else if( childNode.Name.EqualsIgnoreCase( requirementsListNode ) )
                {
                    filesingConfig.GlobalRequireConfigs.Add( LoadRequireConfig( childNode, filesingConfig ) );
                }
            }
        }

        private static void ParsePatterns( XmlNode patternListNode, FilesingConfig filesingConfig )
        {
            foreach( XmlNode childNode in patternListNode.ChildNodes )
            {
                if( childNode.Name.EqualsIgnoreCase( regexNode ) )
                {
                    // Patterns will always default to ignoring case.
                    Regex regex = CreateRegexFromXmlNode( childNode, true );
                    PatternConfig patternConfig = new PatternConfig( regex );
                    filesingConfig.PatternConfigs.Add( patternConfig );
                }
                else if( childNode.Name.EqualsIgnoreCase( patternNode ) )
                {
                    PatternConfig patternConfig = LoadPatternConfig( childNode, filesingConfig );
                    filesingConfig.PatternConfigs.Add( patternConfig );
                }
            }
        }

        private static PatternConfig LoadPatternConfig( XmlNode patternNode, FilesingConfig filesingConfig )
        {
            Regex regex = null;
            List<IgnoreConfig> ignoreConfigs = new List<IgnoreConfig>();
            List<RequireConfig> requireConfigs = new List<RequireConfig>();
            foreach( XmlNode childNode in patternNode.ChildNodes )
            {
                if( childNode.Name.EqualsIgnoreCase( regexNode ) )
                {
                    // Patterns will always default to ignoring case.
                    regex = CreateRegexFromXmlNode( childNode, true );
                }
                else if( childNode.Name.EqualsIgnoreCase( ignoreListNode ) )
                {
                    IgnoreConfig ignoreConfig = LoadIgnoreConfig( childNode, filesingConfig );
                    ignoreConfigs.Add( ignoreConfig );
                }
                else if( childNode.Name.EqualsIgnoreCase( requirementsListNode ) )
                {
                    RequireConfig requireConfig = LoadRequireConfig( childNode, filesingConfig );
                    requireConfigs.Add( requireConfig );
                }
            }

            return new PatternConfig( regex, ignoreConfigs, requireConfigs );
        }

        private static IgnoreConfig LoadIgnoreConfig( XmlNode ignoreNode, FilesingConfig filesingConfig )
        {
            IgnoreConfig ignoreConfig = new IgnoreConfig();

            foreach( XmlNode childNode in ignoreNode.ChildNodes )
            {
                // Parse specific files that are ignored.
                if( childNode.Name.EqualsIgnoreCase( ignoreFileNode ) )
                {
                    ignoreConfig.AddSpecificFileToIgnore(
                        Path.Combine( filesingConfig.SearchDirectoryLocation, childNode.InnerText )
                    );
                }
                // Parse specific dirs that are ignored.
                else if( childNode.Name.EqualsIgnoreCase( ignoreDirNode ) )
                {
                    ignoreConfig.AddSpecificDirToIgnore(
                        Path.Combine( filesingConfig.SearchDirectoryLocation, childNode.InnerText )
                    );
                }
                // Parse files ignored with a regex pattern.
                else if( childNode.Name.EqualsIgnoreCase( ignoreFileWithRegexNode ) )
                {
                    // Files and directories shall always be defaulted to false.
                    Regex regex = CreateRegexFromXmlNode( childNode, false );
                    ignoreConfig.AddFileRegexToIgnore( regex );
                }
                // Parse dirs ignored with a regex pattern.
                else if( childNode.Name.EqualsIgnoreCase( ignoreDirWithRegexNode ) )
                {
                    // Files and directories shall always be defaulted to false.
                    ignoreConfig.AddDirNameToIgnore( childNode.InnerText, LookForIgnoreCaseAttribute( childNode, false ) );
                }
                else if( childNode.Name.EqualsIgnoreCase( ignoreFileWithExtensionNode ) )
                {
                    string pattern = Regex.Escape( childNode.InnerText );

                    // Files and directories shall always be defaulted to false.
                    bool ignoreCase = LookForIgnoreCaseAttribute( childNode, false );

                    RegexOptions options = RegexOptions.Compiled;
                    if( ignoreCase )
                    {
                        options |= RegexOptions.IgnoreCase;
                    }

                    Regex regex = new Regex( pattern, options );
                    ignoreConfig.AddIgnoredFileExtension( regex );
                }
            }

            return ignoreConfig;
        }

        private static RequireConfig LoadRequireConfig( XmlNode requireNode, FilesingConfig filesingConfig )
        {
            RequireConfig config = new RequireConfig();

            foreach( XmlNode childNode in requireNode.ChildNodes )
            {
                // Parse specific file to require
                if( childNode.Name.EqualsIgnoreCase( requireFileNode ) )
                {
                    config.AddRequiredFile(
                        Path.Combine( filesingConfig.SearchDirectoryLocation, childNode.InnerText )
                    );
                }
                // Parse specific dir to require.
                else if( childNode.Name.EqualsIgnoreCase( requireDirNode ) )
                {
                    config.AddRequiredDir(
                        Path.Combine( filesingConfig.SearchDirectoryLocation, childNode.InnerText )
                    );
                }
            }

            return config;
        }

        /// <summary>
        /// Creates a regex from the XML node's value.
        /// </summary>
        /// <param name="node">Node to look for.</param>
        /// <param name="defaultIgnoreCase">
        /// What the default value is to determine if we should ignore the casing
        /// if an ignore case attribute is not specified.  <seealso cref="LookForIgnoreCaseAttribute(XmlNode, bool)"/>
        /// </param>
        private static Regex CreateRegexFromXmlNode( XmlNode node, bool defaultIgnoreCase )
        {
            RegexOptions options = RegexOptions.Compiled;

            // Files and directories shall always be defaulted to false.
            if( LookForIgnoreCaseAttribute( node, defaultIgnoreCase ) )
            {
                options |= RegexOptions.IgnoreCase;
            }

            Regex regex = new Regex( node.InnerText, options );

            return regex;
        }

        /// <summary>
        /// Looks at the given XML node for the "ignorecase" attribute.
        /// Returns true if we should ignore case, else false.
        /// If the attribute is not specified, this function returns the passed in default value.
        /// </summary>
        /// <param name="node">The node to search.</param>
        /// <param name="defaultedValue">What to return if the node has no ignore case attribute.</param>
        private static bool LookForIgnoreCaseAttribute( XmlNode node, bool defaultValue )
        {
            foreach( XmlAttribute attr in node.Attributes )
            {
                if( attr.Name.EqualsIgnoreCase( ignoreCaseAttr ) )
                {
                    return bool.Parse( attr.Value );
                }
            }

            return defaultValue;
        }
    }
}
