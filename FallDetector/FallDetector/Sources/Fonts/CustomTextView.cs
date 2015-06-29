/*
* Copyright (C) 2013 @JamesMontemagno http://www.montemagno.com http://www.refractored.com
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;

using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace FallDetector.Sources.Fonts
{
    public class CustomTextView : TextView
    {
        private const int DaysLater = 0;
        private const int ColorsOfAutumn = 1;
        private const int RemachineScript = 2;

        private TypefaceStyle m_Style = TypefaceStyle.Normal;

        private static readonly Dictionary<int, Typeface> Typefaces = new Dictionary<int, Typeface>(16);

        public CustomTextView(Context context) :
            base(context)
        {
        }

        protected CustomTextView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }


        public CustomTextView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            this.Initialize(context, attrs);
        }

        public CustomTextView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            this.Initialize(context, attrs);
        }

        private void Initialize(Context context, IAttributeSet attrs)
        {


            try
            {
                TypedArray values = context.ObtainStyledAttributes(attrs, Resource.Styleable.CustomTextView);

                int typefaceValue = values.GetInt(Resource.Styleable.CustomTextView_typeface, 0);
                values.Recycle();
                var font = this.ObtainTypeface(context, typefaceValue);
                this.SetTypeface(font, this.m_Style);
            }
            catch (Exception)
            {

            }

        }

        private Typeface ObtainTypeface(Context context, int typefaceValue)
        {
            try
            {

                Typeface typeface = null;
                if (Typefaces.ContainsKey(typefaceValue))
                    typeface = Typefaces[typefaceValue];

                if (typeface == null)
                {
                    typeface = this.CreateTypeface(context, typefaceValue);
                    Typefaces.Add(typefaceValue, typeface);
                }
                return typeface;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        private Typeface CreateTypeface(Context context, int typefaceValue)
        {
            try
            {

                Typeface typeface;
                switch (typefaceValue)
                {
                    case DaysLater:
                        typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/DaysLater.ttf");
                        break;
                    case ColorsOfAutumn:
                        typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/ColorsOfAutumn.ttf");
                        break;
                    case RemachineScript:
                        typeface = Typeface.CreateFromAsset(context.Assets, "Fonts/RemachineScript.ttf");
                        break;
                    default:
                        throw new ArgumentException("Unknown typeface attribute value " + typefaceValue);
                }
                return typeface;

            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}