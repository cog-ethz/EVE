/**
* Copyright (c) 2011-2012 ADInstruments. All rights reserved.
*
* \ADIDatFileSDK_license_start
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, this
*    list of conditions and the following disclaimer.
*
* 2. The name of ADInstruments may not be used to endorse or promote products derived
*    from this software without specific prior written permission.
*
* 3. This is an unsupported product which you use at your own risk. For unofficial 
*    technical support, please use http://www.adinstruments.com/forum .
*
* THIS SOFTWARE IS PROVIDED BY ADINSTRUMENTS "AS IS" AND ANY EXPRESS OR IMPLIED
* WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT ARE
* EXPRESSLY AND SPECIFICALLY DISCLAIMED. IN NO EVENT SHALL ADINSTRUMENTS BE LIABLE FOR
* ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
* \ADIDatFileSDK_license_end
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

/*
 * Note that the ADIDatIOWin.dll is a 32 bit dll, while ADIDatIOWin64.dll is a 64 bit dll.
 * This means ADIDatIOWin.dll can only be loaded as an inprocess server into 32 bit .NET processes, 
 * while ADIDatIOWin64.dll can only be loaded into 64 bit .NET processes. 
 * Therefore it is necessary to change the Platform of .NET applications using these dlls
 * to x64 or Win32 depending on whether you are using the 32 or 64 bit SDK, rather than using Any CPU,
 * otherwise a .NET exception reporting that the dll has not been registered will be thrown when the 
 * application attempts to load the dll.
 */


namespace ADIData
   {
   using ADIDatIOWinLib;


   public class FloatIter : IEnumerator<float>
      {
      public FloatIter(IEnumFloatEx enumIn)
         {
         mEnum = enumIn;
         if (mEnum != null)
            mEnumPos = mEnum.GetPosition();
         }

      public void Dispose()
         {
         if (mEnum != null)
            Marshal.ReleaseComObject(mEnum);
         }

      bool HasEnum()
         {
         return mEnum != null;
         }


      public IEnumerator<float> GetEnumerator() { return this; }

      public float Current
         {
         get
            {
            return mBuf[mCurrIdx];
            }
         }

      object IEnumerator.Current { get { return Current; } }

      public bool MoveNext()
         {
         if (++mCurrIdx >= mNPrevRet)
            return FillBufferForward(); //returns true if any new data put in mBuf
         return true;
         }

      public bool MoveNext(int n)
         {
         int skipped = Skip(n);
         return skipped == n;
         }

      public bool MovePrev()
         {
         if (--mCurrIdx < 0)
            return FillBufferBackward();
         return true;
         }

      public void Reset()
         {
         if (mEnum != null)
            mEnum.Reset();
         mNPrevRet = 0;
         mCurrIdx = -1;
         mEnumPos = 0;
         mEnumPos = mEnum.GetPosition();
         }

      public int Position
         {
         get { return mEnumPos - (mNPrevRet - mCurrIdx); }
         }

      public int GetPosition()
         {
         return mEnumPos - (mNPrevRet - mCurrIdx);
         }

      public int Skip(int n)
         {
         if (mCurrIdx < 0)
            {//mBuffer does not yet contain data, see if we can get some
            FillBufferForward(); //Treat Skip() as a variant of MoveNext()
            }

         if (n >= mNPrevRet - mCurrIdx || n < mCurrIdx)
            {//Skipping outside range of buffered samples
            int skipped = 0; //no. of valid samples skipped
            int nToSkip = n - (mNPrevRet - mCurrIdx);//(mEnd-mOut); //reduce by the number of in buffer samples skipped.
            int enumSkipped = 0;
            if (mEnum != null)
               {
              // mHrIsFilling = mEnum.SkipEx(nToSkip, out enumSkipped);
               mEnumPos += enumSkipped;
               skipped = enumSkipped + (mNPrevRet - mCurrIdx); //increase by the number of in buffer samples skipped.
               }
            if (enumSkipped == nToSkip)
               {//Skip was successful
               FillBufferForward(); //Treat Skip() as a variant of MoveNext()
               //mCurrIdx = -1;   //Alternatively, assume MoveNext() is called after Skip(), before Current()
               //mNPrevRet = 0;
               return skipped;
               }

            //mOut = mEnd;  //Limit position for +ve and -ve skips
            //mStart = mEnd;  //For mOut<mEnd, -ve skip failed, but we don't limit position,
            //mNPrevRet = 0;  //so that, e.g.  for(;mIter;mIter-=2); will terminate
            mCurrIdx = -1;
            mNPrevRet = 0;
            return skipped;
            }
         else
            mCurrIdx += n; //Skip within buffer   
         return n;
         }

      public const int kBufferLen = 64;

      private IEnumFloatEx mEnum = null;

      private float[] mBuf = new float[kBufferLen];
      private int mCurrIdx = -1; //no data yet
      private int mNPrevRet = 0;
      private int mEnumPos = 0;
      //private int mHrIsFilling = 0;

      private bool FillBufferForward()
         {
         mNPrevRet = 0;
         mCurrIdx = -1;
         if (mEnum != null)
            {
            //mHrIsFilling = mEnum.Next(kBufferLen, mBuf, out mNPrevRet);
            mEnumPos += mNPrevRet;
            mCurrIdx = 0;
            }
         return mNPrevRet != 0;
         }

      private bool FillBufferBackward()
         {
         if (mEnum != null)
            {
            int nNew = 0;
            int nSkipped;
            mEnum.SkipEx(-(kBufferLen + mNPrevRet), out nSkipped);
            mEnumPos += nSkipped;
            nNew = -nSkipped - mNPrevRet; //no. of new samples available
            if (nNew > 0)
               {
               //mHrIsFilling = mEnum.Next(nNew, mBuf, out mNPrevRet);
               mEnumPos += mNPrevRet;
               mCurrIdx = mNPrevRet - 1;
               return mNPrevRet>0 ? true : false;
               }
            }
         //make iterator invalid
         mNPrevRet = 0;
         mCurrIdx = -1;
         return false;
         }

      }

   public class CommentIter : IEnumerator<IADIComment>
      {
      public CommentIter(IEnumADIComment enumIn)
         {
         mEnum = enumIn;
         }

      public CommentIter(IEnumADIComment enumIn, int endTick)
         {
         mEnum = enumIn;
         mEndTick = endTick;
         }

      public void Dispose()
         {
         if (mCurrent != null)
            Marshal.ReleaseComObject(mCurrent);
         if (mEnum != null)
            Marshal.ReleaseComObject(mEnum);
         }

      bool HasEnum()
         {
         return mEnum != null;
         }


      public IEnumerator<IADIComment> GetEnumerator() { return this; }

      public IADIComment Current
         {
         get
            {
            return mCurrent;
            }
         }

      object IEnumerator.Current { get { return Current; } }

      public bool MoveNext()
         {
         mEnum.Next(1, mEndTick, out mCurrent, out mNPrevRet);

         if (mNPrevRet != 0)
            return true;
         return false;
         }

      public void Reset()
         {
         if (mEnum != null)
            mEnum.Find(EventFindTypes.kEFTFindBackwards, 0, 0, ADIPosition.kRecordEndOffset);
         mNPrevRet = 0;
         }

      public ChartCommentPos Position
         {
         get
            {
            if (mEnum != null)
               return mEnum.GetPosition();
            return kNullPos;
            }
         }

      public void Find(EventFindTypes findType, int startPos, int startLimit, int endLimit)
         {
         if (mEnum != null)
            mEnum.Find(findType, startPos, startLimit, endLimit);
         }

      public static ChartCommentPos kNullPos;

      private IEnumADIComment mEnum = null;

      //private IADIComment[] mBuf = new IADIComment[kBufferLen];
      private IADIComment mCurrent;
      private int mEndTick = ADIPosition.kRecordEndOffset;

      private int mNPrevRet = 0;

      }


   public class ADIDataReader : IDisposable
      {
      /// <summary>
      /// Opens the specified file and creates an ADIDataReader object allowing data to be read from it.
      /// </summary>
      /// <param name="filePath"> path to file to open</param>
      public ADIDataReader(String filePath)
         {
         ADIDatIOWinLib.ADIDataObject dataObject = new ADIDatIOWinLib.ADIDataObject();
         ADIDatIOWinLib.IADIDataReader reader = (ADIDatIOWinLib.IADIDataReader)dataObject;
         reader.OpenFileForRead(filePath);
         mADIData = reader.GetADIData();
         }
      /// <summary>
      /// Closes the file held open by the ADIDataReader object
      /// </summary>
      public void Dispose()
         {
         if (mADIData != null)
            {
            IDisposable dispose = (IDisposable)mADIData;
            dispose.Dispose();
            //Marshal.ReleaseComObject(mADIData);
            }
         //GC.SuppressFinalize(this);
         }

      /// <summary>
      /// Provides access to the raw RCW interface for accessing Labchart data from the file.
      /// </summary>
      public ADIDatIOWinLib.IADIData ADIData
         {
         get { return mADIData; }
         }

      /// <summary>
      /// returns number of blocks (records) in the file.
      /// </summary>
      public int NumberOfRecords
         {
         get
            {
            if (mADIData != null)
               return mADIData.GetNumberOfRecords(ADIDatIOWinLib.ADIDataType.kADIDataRecorded);
            return 0;
            }
         }

      /// <summary>
      /// returns number of channels, i.e. the index of the last channel plus 1. Currently ignores the dataType parameter.
      /// </summary>
      public int NumberOfChannels
         {
         get
            {
            if (mADIData != null)
               return mADIData.GetNumberOfChannels(ADIDatIOWinLib.ADIDataType.kADIDataRecorded);
            return 0;
            }
         }

      /// <summary>
      /// returns boolean indicating whether or not the specified channel and record contains data.
      /// </summary>
      /// <param name="chan">channel index (0 based)</param>
      /// <param name="rec">record index (0 based)</param>
      /// <returns>true if data present</returns>
      public bool RecordHasData(int chan, int rec)
         {
         return mADIData.RecordHasData(new ADIChannelId(chan), rec);
         }

      /// <summary>
      /// returns the number of ticks (fastest samples) in the record.      
      /// </summary>
      /// <param name="rec">record index (0 based)</param>
      /// <returns>number of ticks in record</returns>
      public int GetRecordLength(int rec)
         {
         return mADIData.GetRecordLength(ADIReservedFlags.kADIReservedNil, rec);
         }

      //If dataFlags does not have kADIDataAtTickRate set (i.e. kADIDataAtSampleRate), returns the actual number of recorded samples in this record and channel.
      //If dataFlags has kADIDataAtTickRate set, returns the number of recorded samples transformed into ticks, which may not agree with the number of ticks returned by GetRecordLength().
      public int GetNumSamplesInRecord(ADIDataFlags dataFlags, int chan, int rec)
         {
         return mADIData.GetNumSamplesInRecord(dataFlags, new ADIChannelId(chan), rec);
         }

      /// <summary>
      /// Returns the period in seconds of the samples in the specified channel for the specified record
      /// </summary>
      /// <param name="dataFlags">Specifies the type of data to return information about (tick rate vs sample rate)</param>
      /// <param name="chan">channel index (0 based)</param>
      /// <param name="rec">record index (0 based)</param>
      /// <returns>period in seconds</returns>
      public double GetSecsPerSample(ADIDataFlags dataFlags, int chan, int rec)
         {
         return mADIData.GetSecsPerSample(ADIDataFlags.kADIDataAtSampleRate, new ADIChannelId(chan), rec);
         }

      /// <summary>
      /// Returns the period in seconds of the samples in the fastest channel (ticks) for the specified record
      /// </summary>
      /// <param name="rec">Index of record (block)</param>
      /// <returns> seconds per tick</returns>
      public double GetSecsPerTick(int rec)
         {
         return mADIData.GetSecsPerSample(ADIDataFlags.kADIDataAtTickRate, new ADIChannelId(0), rec);
         }

      /// <summary>
      /// Returns a TTickToSample struct containing the linear transform mapping a tick within a record to a sample
      /// within that record and specified channel. 
      /// This linear transform will be the identity transform unless the file contains multirate data. 
      /// </summary>
      /// <param name="dataFlags"> This parameter is currently ignored.</param>
      /// <param name="channel">channel index (0 based)</param>
      /// <param name="rec">record index (0 based)</param>
      /// <returns>TTickToSample value type containing linear transform</returns>
      public TTickToSample GetTickToSample( ADIDataFlags dataFlags, ADIChannelId channel, int rec)
         {
         return GetTickToSample(dataFlags, channel, rec);
         }


      /// <summary>
      /// Returns the time and date of the zero time (origin) for the specified record.
      /// </summary>
      /// <param name="rec">Index of record (block)</param>
      /// <returns>Time and date</returns>
      public DateTime GetRecordTriggerTime(int rec)
         {
         RecordTimeInfo timeInfo = new RecordTimeInfo();
         timeInfo = mADIData.GetRecordTimeInfo(ADIReservedFlags.kADIReservedNil, rec, Marshal.SizeOf(timeInfo));

         return ADITimeToDateTime(timeInfo.mRecordTriggerTime.mSeconds);
         }

      /// <summary>
      /// Returns the time and date of the first tick in the specified record
      /// </summary>
      /// <param name="rec">Index of record (block)</param>
      /// <returns> Time and date </returns>
      public DateTime GetRecordStartTime(int rec)
         {
         RecordTimeInfo timeInfo = new RecordTimeInfo();
         timeInfo = mADIData.GetRecordTimeInfo(ADIReservedFlags.kADIReservedNil, rec, Marshal.SizeOf(timeInfo));

         return ADITimeToDateTime(timeInfo.mRecordTriggerTime.mSeconds).AddSeconds(-(timeInfo.mSecPerTick * timeInfo.mTrigTickMinusRecStartTick));
         }

      public String ChannelName(int chan)
         {
         IAutoADIString adiStr = mADIData.GetChannelName(new ADIChannelId(chan));
         return adiStr != null ? adiStr.GetBStr() : "";
         }

      public String UnitsName(int chan, int rec)
         {
         IAutoADIString adiStr = mADIData.GetUnitsName(ADIDataFlags.kADIDataScaleToUnits, new ADIChannelId(chan), rec);
         return adiStr != null ? adiStr.GetBStr() : "";
         }

      public String UnitsName(int chan, int rec, ADIDataFlags scalingType)
         {
         IAutoADIString adiStr = mADIData.GetUnitsName(scalingType, new ADIChannelId(chan), rec);
         return adiStr != null ? adiStr.GetBStr() : "";
         }

      public String PrefixedUnitsName(int chan, int rec)
         {
         IAutoADIString adiStr = mADIData.GetUnitsName(ADIDataFlags.kADIDataScaleToPrefixedUnits, new ADIChannelId(chan), rec);
         return adiStr != null ? adiStr.GetBStr() : "";
         }


      //      public System.Collections.IEnumerable RecordIterator(ADIDataFlags dataFlags, int chan, ADIPosition pos)
      //      public IEnumerator<float> RecordIterator(ADIDataFlags dataFlags, int chan, ADIPosition pos)

      //Creates a object to enumerate samples in the record from left to right starting at position pos and ending at the sample before endOffset.
      //If the parameter endOffset is set to ADIPosition.kRecordEndOffset, the enumerator will continue to return samples until the last sample in the record is reached.
      public FloatIter RecordIterator(ADIDataFlags dataFlags, int chan, ADIPosition pos, int endOffset)
         {
         ADIScaling scaling = new ADIScaling(1); //identity scaling
         IEnumFloatEx enumFloat;
         mADIData.GetEnumFloat(dataFlags, new ADIChannelId(chan), pos, endOffset, ref scaling, out enumFloat); //(ADIScaling*)IntPtr.Zero);
         return new FloatIter(enumFloat);
         }
      
      //Returns a single data value. This not an efficient way to access ranges of samples, for which
      //RecordIterator() should be used.
      //If the specified channel and record is empty the value returned will be
      //a nan (0x7ff8700000000000LL). If pos.mRecordOffset is beyond the end of the specified record an exception
      //(E_INVALIDARG) will be thrown.
      public double GetValue(ADIDataFlags dataFlags, int chan, ADIPosition pos)
         {
         ADIScaling scaling = new ADIScaling(1); //identity scaling
         return mADIData.GetValDouble(dataFlags, new ADIChannelId(chan), pos, ref scaling); //(ADIScaling*)IntPtr.Zero);
         }

      //Creates a object to enumerate comments in the record from left to right starting at position start and ending at (but not including) stop.
      //If the parameter stopTick is set to ADIPosition.kRecordEndOffset, the enumerator will continue to return comments until the last comment in the record is reached.
      //The chan parameter is ignored if the searchFlags parameter has kSearchAnyChannel set.
      public CommentIter CommentIterator(EnumCommentFlags searchFlags, int chan, ADIPosition startPos, int stopTick)
         {
         IEnumADIComment enumComments;
         mADIData.CreateEnumComment(searchFlags, chan, startPos, stopTick, 0, out enumComments);
         return new CommentIter(enumComments);
         }

      public CommentIter CommentIterator(ADIPosition startPos, int stopTick)
         {
         IEnumADIComment enumComments;
         mADIData.CreateEnumComment(EnumCommentFlags.kSearchAnyChannel, 0, startPos, stopTick, 0, out enumComments);
         return new CommentIter(enumComments);
         }

      //Creates a object to enumerate comments in the record from right to left starting at position start and ending at (and including) stop.
      //If the parameter start is set to ADIPosition.kRecordEndOffset, the enumerator will return comments starting from the end of the record.
      //The channel parameter is ignored if the flags parameter has kSearchAnyChannel set.
      public CommentIter CommentIteratorReverse(EnumCommentFlags searchFlags, int chan, ADIPosition startPos, int stopTick)
         {
         IEnumADIComment enumComments;
         mADIData.CreateEnumCommentReverse(searchFlags, chan, startPos, stopTick, 0, out enumComments);
         return new CommentIter(enumComments, stopTick);
         }


      //Searches from right to left for a comment in the record starting at (but not including) position rightPos and ending at (and including) leftPos.
      //The channel parameter is ignored if the flags parameter has kSearchAnyChannel set.
      //[helpstring("method GetRightMostCommentInRange: searches back from rightPos for the first comment to the left of rightPos.")] 
      //   HRESULT GetRightMostCommentInRange([in]EnumCommentFlags searchFlags, [in]ADIPosition leftPos, [in]ADIPosition rightPos, [in]TChanIndex chan, [out]IADIComment **icmt) const;
      public IADIComment RightMostCommentInRange(EnumCommentFlags searchFlags, ADIPosition leftPos, ADIPosition rightPos, int chan)
         {
         IADIComment comment;
         mADIData.GetRightMostCommentInRange(searchFlags, leftPos, rightPos, chan, out comment);
         return comment;
         }

      protected ADIDatIOWinLib.IADIData mADIData;

      public static DateTime ADITimeToDateTime(double secsSince1970)
         {
         DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
         return dt.AddSeconds(secsSince1970);
         }

      }
   } //namespace
