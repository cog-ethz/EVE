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

// Edited a bit by Katja Wolff


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ADIData;
using ADIDatIOWinLib;

namespace ADIDatRead
   {
   class ShowChannelStats 
      {
      public static void ShowStats(ADIDataReader reader)
         {
         int nChannels = reader.NumberOfChannels;
         int nRecords = reader.NumberOfRecords;
         Debug.Log("Number of records = " + nRecords);
         Debug.Log("Number of channels = " + nChannels);

         //loop over channels, displaying their names
         for (int chan = 0; chan < nChannels; ++chan)
            {
            Debug.Log("Channel " + (chan+1) + " name is " + reader.ChannelName(chan));
            }

         for (int record = 0; record < nRecords; ++record)
            {
            //print the time and date of the first tick in the record, and of the origin of the record 
            //(in case there is a pre or post trigger delay).
            DateTime startTime = reader.GetRecordStartTime(record);
            Debug.Log("Block start time:" + startTime + ", Block zero time:" + reader.GetRecordTriggerTime(record));

            //print the length of the record in ticks
            Debug.Log("Record " + (record+1) + " is " + reader.GetRecordLength(record) + " ticks long.");

            //Output all the comments in the record (including all channels and AllChannel comments).
            //Start iterator at the beginning (i.e. record offset 0) of record rec.
/*            using (CommentIter comments = reader.CommentIterator(new ADIPosition(record, 0), ADIPosition.kRecordEndOffset))
               {
               foreach (IADIComment comment in comments)
                  {
                  int pos, chan, num;
                  String text = comment.GetCommentInfo(out pos, out chan, out num).GetBStr();
                  if(chan == -1)
                     Debug.Log("Comment " + num + ", All Channels: " + (chan+1) + " \"" + text + "\"");
                  else
                     Debug.Log("Comment " + num + ", Chan " + (chan+1) + ": \"" + text + "\"");

                  //Get the value of the channel data at the comment if possible
                  ADIDataFlags dataFlags = ADIDataFlags.kADIDataAtTickRate | ADIDataFlags.kADIDataScaleToPrefixedUnits;
                  double value = reader.GetValue(dataFlags, chan, new ADIPosition(record, pos));

                  String unitsName = reader.UnitsName(chan, record, dataFlags);
                  Debug.Log("Value at comment= " + value + " " + unitsName);
                  }
               }
*/

            //Demonstrate using the reverse comment iterator and finding individual comments
            //Output 2nd last comment in channel 3
/*            ADIPosition rightPos = new ADIPosition(record, ADIPosition.kRecordEndOffset);
            int stopTick = 0; //Iterator can run right back to the start of the record
            using (CommentIter comments = reader.CommentIteratorReverse(EnumCommentFlags.kSearchSpecificChannelOnly, 
               2, rightPos, stopTick))
               {
               int count = 0;
               foreach (IADIComment comment in comments)
                  {
                  int pos, chan, num;
                  if (++count == 2)
                     {
                     String text = comment.GetCommentInfo(out pos, out chan, out num).GetBStr();
                     Debug.Log("2nd last comment in record: Comment " + num + " Chan " + (chan+1) + " \"" + text + "\"");
                     break;
                     }
                  }
               //Alternative to foreach:
               //for(;comments.MoveNext();)
               //   {
               //   IADIComment comment = comments.Current;
               //   int pos, chan, num;
               //   String text = comment.GetCommentInfo(out pos, out chan, out num).GetBStr();
               //   Debug.Log("2nd last Comment " + num + " Chan " + chan + " \"" + text + "\"");
               //   }
               }
*/

            //Find the right-most comment in this record.
            IADIComment rightMostComment = reader.RightMostCommentInRange(EnumCommentFlags.kSearchAnyChannel, 
               new ADIPosition(record, 0), new ADIPosition(record, ADIPosition.kRecordEndOffset), 0);
            if (rightMostComment != null)
               {
               int pos, chan, num;
               String text = rightMostComment.GetCommentInfo(out pos, out chan, out num).GetBStr();

               Debug.Log("Right-most in record: Comment " + num + " Chan " + (chan+1) + " \"" + text + "\"");
               }


            for (int channel = 0; channel < nChannels; ++channel)
               {
               //Specify that we want data at the raw sample rate of the channel (not the tick rate), in prefixed units such as mV (rather than V, say).
               ADIDataFlags dataFlags = ADIDataFlags.kADIDataAtSampleRate | ADIDataFlags.kADIDataScaleToPrefixedUnits;

               int samplesInRecord = reader.GetNumSamplesInRecord(dataFlags, channel, record);
               double samplePeriod = reader.GetSecsPerSample(dataFlags, channel, record);
               Debug.Log("Channel "+(channel+1)+", record "+record+" contains "+samplesInRecord+" samples with a sample period of "+samplePeriod+" s.");

               //Demonstrate that records can start part-way through a sample in multi-rate files
               TTickToSample tickToSample = reader.ADIData.GetTickToSample(dataFlags, new ADIChannelId(channel), record);
               double samplePosInRecord = tickToSample.TickToSample(0);
               if(samplePosInRecord != 0.0)
                  {
                  Debug.Log("First tick in channel has sample position "+samplePosInRecord+".");
                  Debug.Log("I.e. the record  starts "+(samplePosInRecord*100.0)+" percent through the time period of the 1st sample in this channel.");
                  }
               Debug.Log("Last sample in channel starts at tick "+tickToSample.SampleToTick(samplesInRecord));

               //Print the units for the channel and record
               String channelName = reader.ChannelName(channel);
               String unitsName = reader.UnitsName(channel, record, dataFlags);
               Debug.Log(channelName + " units [" + unitsName + ']');

               //Process the samples in the channel and record
               using (FloatIter iter = reader.RecordIterator(dataFlags, channel, new ADIPosition(record, 0), ADIPosition.kRecordEndOffset))
                  {
                  double sum = 0.0;
                  int samples = 0;
                  double minVal = double.MaxValue;
                  double maxVal = double.MinValue;
                  foreach(float val in iter)
                     {
                     sum += val;
                     samples++;
                     if (val > maxVal)
                        maxVal = val;
                     if (val < minVal)
                        minVal = val;
                     }
                  int samplePos = iter.GetPosition();
                  if(samplePos > 0)
                     {
                     if (samplePos != samples)
                        throw new Exception("Unexpected mismatch: iter.GetPosition() error");

                     double mean = sum/samplePos; //samplePos should equal the number of samples since it started at 0.
                     Debug.Log("Channel "+(channel+1)+", record "+(record+1)+" : sum="+sum+",samples="+samples+
                        ", mean= "+mean+" "+unitsName+", max= "+maxVal+" "+unitsName+", min= "+minVal+" "+unitsName);
                     }
                  else
                     Debug.Log("Channel " + (channel + 1) + ", record " + (record + 1) + " has no data");
                  }

               //Example demonstrating driving iterator backwards
               using (FloatIter iter = reader.RecordIterator(dataFlags, channel, new ADIPosition(record, samplesInRecord), ADIPosition.kRecordEndOffset))
                  {
                  double sum = 0.0;
                  int samples = 0;
                  for (; iter.MovePrev(); )
                     {
                     sum += iter.Current;
                     samples++;
                     }
                  Debug.Log("Reverse sum=" + sum+", samples = "+samples);
                  }
               }
            }
         }
      }
   }
