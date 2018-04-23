using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomAvatarSelector : MonoBehaviour {

	[Header("Male", order=1)]
	public bool maleTough;
	public bool maleSlim;
	public bool maleLarge;
	public bool maleSenior;
	public bool boy;
	
	[Header("Female", order=1)]
	public bool femaleBeach;
    public bool femaleSummer;
	public bool femaleSlim;
	public bool femaleLarge;
	public bool femaleSenior;
	public bool girl;

    [Header("Pose", order = 1)]
    public bool walking_standing;
    public bool sitting;
    public bool wallLean;
    public bool talk1;
    public bool talk2;

    [Header("Quality", order=1)]
	//public bool high = true;
	public bool circleAround = true;
    public bool moving = true;

	private ArrayList prefabNames;

	private GameObject avatar, player;
	private AgentGoToNTargets waypoints;

	private List<string> avatarNames; //contains names of all prefabs from which we could choose
	public List<Transform> targetList;

	private List<int> uniformRanges; //contains indices for each group of enabled avatars, to uniformly select a group and then uniformly select a variant of that group
	
	// Use this for initialization
	void Start () {
        
        player = GameObject.FindGameObjectsWithTag("Player")[0];

		/*
		 * Get possible avatars from user selection
		 */
		avatarNames = new List<string> ();
		uniformRanges = new List<int> ();
		if (boy) {
			for (int i = 1; i <10; i++){
				avatarNames.Add("Boys/Boy_h_0" + i);
			}
			avatarNames.Add("Boys/Boy_h_10");
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(10);
		}
		
		if (girl) {
			for (int i = 1; i <10; i++){
				avatarNames.Add("Girls/Pants/Girl_p_h_0" + i);
				avatarNames.Add("Girls/Skirts/Girl_s_h_0" + i);
			}
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(18);
		}

        if (femaleLarge)
        {
            for (int i = 1; i < 21; i++)
            {
                avatarNames.Add("Females/Large/Woman_lar_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(20);
        }

        if (femaleSlim)
        {
            for (int i = 1; i < 19; i++)
            {
                avatarNames.Add("Females/Slim/Woman_sli_h_" + i.ToString("D2"));
            }
            avatarNames.Add("Females/Slim/Woman_sli_h_20");
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(19);
        }

        if (femaleBeach)
        {
            for (int i = 1; i < 6; i++)
            {
                avatarNames.Add("Females/Summer/Woman_sum_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(5);
        }

        if (femaleSummer)
        {
            for (int i = 6; i < 13; i++)
            {
                avatarNames.Add("Females/Summer/Woman_sum_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(7);
        }


        if (maleLarge)
        {
            for (int i = 1; i < 21; i++)
            {
                avatarNames.Add("Males/Large/Man_lar_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(20);
        }

        if (maleSlim)
        {
            for (int i = 1; i < 21; i++)
            {
                avatarNames.Add("Males/Slim/Man_sli_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(20);
        }
        
        if (maleTough)
        {
            for (int i = 1; i < 21; i++)
            {
                avatarNames.Add("Males/Tough/Man_tou_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(20);
        }

        if (maleSenior)
        {
            for (int i = 1; i < 6; i++)
            {
                avatarNames.Add("Seniors/Men/Man_senior_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(5);
        }

        if (femaleSenior)
        {
            for (int i = 1; i < 6; i++)
            {
                avatarNames.Add("Seniors/Women/Woman_senior_h_" + i.ToString("D2"));
            }
            uniformRanges.Add(avatarNames.Count);
            //uniformRanges.Add(5);
        }
        

		/*
		 * Get random avatar from the user selection. the distribution among selected groups is uniform.
		 */
		int offsetIndex = Random.Range(0,uniformRanges.Count);//randomly select the range in which we will select a random avatar
		int index = -1;
		if (offsetIndex == 0) {
			index = Random.Range (0, uniformRanges[0]);//randomly select a variant of the avatar group
		} else {
			index = Random.Range (uniformRanges[offsetIndex-1], uniformRanges[offsetIndex]);//randomly select a variant of the avatar group
		}
      //  Debug.Log(avatarNames[index]);

        avatar = Instantiate(Resources.Load<GameObject>(avatarNames[index])) as GameObject;
        Animator animator = avatar.gameObject.GetComponent<Animator>();
        if (walking_standing)
            animator.runtimeAnimatorController = Resources.Load("NLocomotion") as RuntimeAnimatorController;
        else if (sitting & (avatarNames[index].Contains("Woman")))
            animator.runtimeAnimatorController = Resources.Load("FemaleSitting") as RuntimeAnimatorController;
        else if (sitting)
            animator.runtimeAnimatorController = Resources.Load("MaleSitting") as RuntimeAnimatorController;
        else if (wallLean)
            animator.runtimeAnimatorController = Resources.Load("WallLean") as RuntimeAnimatorController;
        else if (talk1 & (avatarNames[index].Contains("Woman")))
            animator.runtimeAnimatorController = Resources.Load("FemaleTalk1") as RuntimeAnimatorController;
        else if (talk2 & (avatarNames[index].Contains("Woman")))
            animator.runtimeAnimatorController = Resources.Load("FemaleTalk2") as RuntimeAnimatorController;
        else if (talk1 & (avatarNames[index].Contains("Girls") || avatarNames[index].Contains("Boy")))
            animator.runtimeAnimatorController = Resources.Load("KidsTalk1") as RuntimeAnimatorController;
        else if (talk2 & (avatarNames[index].Contains("Girls") || avatarNames[index].Contains("Boy")))
            animator.runtimeAnimatorController = Resources.Load("KidsTalk2") as RuntimeAnimatorController;
        else if (talk1)
            animator.runtimeAnimatorController = Resources.Load("MaleTalk1") as RuntimeAnimatorController;
        else if (talk2)
            animator.runtimeAnimatorController = Resources.Load("MaleTalk2") as RuntimeAnimatorController;
        GameObject placeHolder = transform.Find("PlaceHolder").gameObject;
		avatar.transform.position = placeHolder.transform.position; //place avatar at correct start location
        avatar.transform.rotation = placeHolder.transform.rotation;

		/*
		 * Get Targets 
		 */
		waypoints = avatar.GetComponent<AgentGoToNTargets> ();
		waypoints.circleAround = circleAround;
		/*targetList = new List<Transform>();
		foreach (Transform child in transform) {
			if (child.transform.CompareTag ("NPCWalkTarget")) {
				targetList.Add(child);
			}
		}*/
        targetList = shuffleTargetList(targetList);
		waypoints.targets = targetList.ToArray();//

		/*
		 * Clean up avatar and remove placeholder
		 */
		avatar.GetComponent<Rigidbody> ().detectCollisions = false;
		avatar.GetComponent<BoxCollider> ().enabled = false;
        
        if (talk1 || talk2 || sitting || wallLean)
        {
            //avatar.GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            avatar.GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            avatar.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        }
            
        avatar.GetComponent<AgentGoToNTargets>().moving = moving;
        avatar.GetComponent<AgentGoToNTargets>().player = player.transform;

		GameObject.Destroy (placeHolder);
	}

    private static System.Random rng = new System.Random();

    public List<Transform> shuffleTargetList(List<Transform> list)
    {
        List<Transform> result = list;
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Transform value = result[k];
            result[k] = result[n];
            result[n] = value;
        }
        return result;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
