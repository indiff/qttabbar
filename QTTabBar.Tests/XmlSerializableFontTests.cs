using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QTTabBarLib;

namespace QTTabBar.Tests {
    [TestClass]
    public class XmlSerializableFontTests {
        [TestMethod]
        public void FromFont_NullInput_ReturnsNull() {
            Assert.IsNull(XmlSerializableFont.FromFont(null));
        }

        [TestMethod]
        public void FromFont_PreservesFontName() {
            using (Font font = new Font("Arial", 10f)) {
                XmlSerializableFont result = XmlSerializableFont.FromFont(font);
                Assert.AreEqual("Arial", result.FontName);
            }
        }

        [TestMethod]
        public void FromFont_PreservesFontSize() {
            using (Font font = new Font("Arial", 12f)) {
                XmlSerializableFont result = XmlSerializableFont.FromFont(font);
                Assert.AreEqual(12f, result.FontSize);
            }
        }

        [TestMethod]
        public void FromFont_PreservesBoldStyle() {
            using (Font font = new Font("Arial", 10f, FontStyle.Bold)) {
                XmlSerializableFont result = XmlSerializableFont.FromFont(font);
                Assert.AreEqual(FontStyle.Bold, result.FontStyle);
            }
        }

        [TestMethod]
        public void FromFont_PreservesItalicStyle() {
            using (Font font = new Font("Arial", 10f, FontStyle.Italic)) {
                XmlSerializableFont result = XmlSerializableFont.FromFont(font);
                Assert.AreEqual(FontStyle.Italic, result.FontStyle);
            }
        }

        [TestMethod]
        public void ToFont_RoundTrip_PreservesNameAndSize() {
            using (Font original = new Font("Arial", 9f)) {
                XmlSerializableFont xsf = XmlSerializableFont.FromFont(original);
                using (Font restored = xsf.ToFont()) {
                    Assert.AreEqual(original.Name, restored.Name);
                    Assert.AreEqual(original.Size, restored.Size, 0.001f);
                }
            }
        }

        [TestMethod]
        public void ToFont_RoundTrip_PreservesStyle() {
            using (Font original = new Font("Arial", 9f, FontStyle.Bold | FontStyle.Italic)) {
                XmlSerializableFont xsf = XmlSerializableFont.FromFont(original);
                using (Font restored = xsf.ToFont()) {
                    Assert.AreEqual(original.Style, restored.Style);
                }
            }
        }

        [TestMethod]
        public void ToFont_Static_RoundTrip() {
            using (Font original = new Font("Arial", 11f, FontStyle.Regular)) {
                XmlSerializableFont xsf = XmlSerializableFont.FromFont(original);
                using (Font restored = XmlSerializableFont.ToFont(xsf)) {
                    Assert.AreEqual(original.Name, restored.Name);
                    Assert.AreEqual(original.Size, restored.Size, 0.001f);
                    Assert.AreEqual(original.Style, restored.Style);
                }
            }
        }
    }
}
