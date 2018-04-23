using UnityEngine;
using System.Collections;

public class GSRCOMWrapper {

	public GSRCOMWrapper() {

	}


}


/*


#biopac imports
import comtypes.client
from comtypes.automation import IDispatch
import win32com.client
		
		#vizard imports
		import viz
		
		
		
		class CheapPowerlabWrapper(viz.EventClass):
		def __init__(self):
		#init vizard event
		viz.EventClass.__init__(self)
		#init com event
		self.application=win32com.client.DispatchWithEvents('ADIChart.Document', ADIEvents)
		comtypes.CoInitialize()		
		self.startSampling()
		self.callback(viz.UPDATE_EVENT,self.newSamples)
		
		def startSampling(self):
		self.application.StartSampling()
		
		def newSamples(self,e):
		self.application.GetChannelName(1)
		
		def getGSR(self):
		"""
		Gets all GSR events since the last query, deletes the queue in
		the ADIEvent.
		"""
		result = self.application.GSR
		self.application.GSR = []
		return result  
		
		
		class ADIEvents():
		
		def __init__(self):
		self.GSR = []
		self.numSamples = 0
		
		def OnStartSampling(self):
		print "start sampling"
		
		def OnNewSamples(self,s):
		"""
		s is the number of samples since last retrieval
		"""
		self.numSamples += s
		#GSR channel
		self.GSR.append(self.GetChannelData(1,1, 1,self.numSamples,-1)[0])
		#print self.GSR
		
		#unused channels - need different sensor than GSR to use these channels
		#c2=self.GetChannelData(1,2, 1,numsamp,-1)[0]
		#c3=self.GetChannelData(1,3, 1,numsamp,-1)[0]
		#c4=self.GetChannelData(1,4, 1,numsamp,-1)[0]

*/
