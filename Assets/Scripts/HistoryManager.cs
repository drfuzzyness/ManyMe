using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ManyMeMode { Rewind, Snapshop, Off };

public class HistoryManager : MonoBehaviour {
	
	public int framerate;
	public int outputTime;
	public ManyMeMode mode;
	public bool thirdSelf;
	//public int number;
	private int outputIndex;

	private KinectManager manager;

	
	// rectangle taken by the foreground texture (in pixels)
	private Rect liveRect;
	private Rect rewindRect;
	private Rect rewindRect2;
	//private Vector2 foregroundOfs;

	private List<GameObject> liveJoints;
	//private List<List<GameObject>> positionHistory;
	private Queue<Color[]> textureHistory;
	private Queue<Color[]> textureHistory2;
	private List<Texture2D> snapshots;
	private List<Rect> snapshotRects;

	// the foreground texture
	private Texture2D liveTexture;
	private Texture2D rewindTexture;
	private Texture2D rewindTexture2;

	void Awake() {
		Application.targetFrameRate = framerate;
	}

	// Use this for initialization
	void Start () {
		textureHistory = new Queue<Color[]>();
		textureHistory2 = new Queue<Color[]>();
		snapshots = new List<Texture2D>();
		snapshotRects = new List<Rect>();


		manager = KinectManager.Instance;
		// calculate the foreground rectangle

		liveRect = PrepareNewGUITexture();
		rewindRect = PrepareNewGUITexture();
		rewindRect2 = PrepareNewGUITexture();

		
		// create joint colliders
		//int numColliders = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		//liveJoints = new GameObject[numColliders];
		liveTexture = manager.GetUsersLblTex();
		rewindTexture = new Texture2D( liveTexture.width, liveTexture.height );
		rewindTexture2 = new Texture2D( liveTexture.width, liveTexture.height );
		if( mode == ManyMeMode.Rewind ) {
			StartCoroutine( RewindTexture() );
		}
		else if( mode == ManyMeMode.Snapshop ) {
			//StartCoroutine( );
		}
	}
	
	// Update is called once per frame
	void Update () {
		outputIndex = outputTime * framerate;
		if(manager == null)
			manager = KinectManager.Instance;
		if( mode == ManyMeMode.Snapshop ) {
			if( Input.GetKeyDown( KeyCode.Space ) ) {
				snapshots.Add( new Texture2D( liveTexture.width, liveTexture.height ) );
				Color[] savedColor = liveTexture.GetPixels();
				snapshots[snapshots.Count-1].SetPixels( savedColor );
				snapshotRects.Add( PrepareNewGUITexture() );
			} else if( Input.GetKeyDown( KeyCode.LeftAlt ) || Input.GetKeyDown( KeyCode.RightAlt ) ) {
				snapshots.Clear();
				snapshotRects.Clear();
			}
		}
		/*if(manager.IsUserDetected()) {
			uint userId = manager.GetPlayer1ID();
			int numColliders = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

			for(int i = 0; i < numColliders; i++) {
				if(manager.IsJointTracked(userId, i)) {
					Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, i);
					
					if(posJoint != Vector3.zero) {
						// convert the joint 3d position to depth 2d coordinates
						Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);
						
						float scaledX = posDepth.x * foregroundRect.width / KinectWrapper.Constants.DepthImageWidth;
						float scaledY = posDepth.y * -foregroundRect.height / KinectWrapper.Constants.DepthImageHeight;
						
						float screenX = foregroundOfs.x + scaledX;
						float screenY = Camera.main.pixelHeight - (foregroundOfs.y + scaledY);
						float zDistance = posJoint.z - Camera.main.transform.position.z;
						
						Vector3 posScreen = new Vector3(screenX, screenY, zDistance);
						Vector3 posCollider = Camera.main.ScreenToWorldPoint(posScreen);
						
						// jointColliders[i].transform.position = posCollider;
					}
				} 
			}
		}*/
	}
	
		
	IEnumerator RewindTexture() {
		while (true) {
			liveTexture = manager.GetUsersLblTex();
			rewindTexture = new Texture2D( liveTexture.width, liveTexture.height );
			rewindTexture2 = new Texture2D( liveTexture.width, liveTexture.height );
			textureHistory.Enqueue(  liveTexture.GetPixels() );
			if( textureHistory.Count > outputIndex ) {
				textureHistory2.Enqueue( textureHistory.Peek() );
				rewindTexture.SetPixels( textureHistory.Dequeue() );
			} else {
				rewindTexture = liveTexture;
			}
			if( textureHistory2.Count > outputIndex ) {
				rewindTexture2.SetPixels( textureHistory2.Dequeue() );
			} else {
				rewindTexture2 = liveTexture;
			}

			//Debug.Log("Frame Done. Stored: " + textureHistory.Count);

			yield return new WaitForSeconds(1.0f/(float)framerate);
		}
	}

	public static Rect PrepareNewGUITexture() {
		Rect cameraRect = Camera.main.pixelRect;
		float rectHeight = cameraRect.height;
		float rectWidth = cameraRect.width;
		if(rectWidth > rectHeight){
			rectWidth = rectHeight * KinectWrapper.Constants.DepthImageWidth / KinectWrapper.Constants.DepthImageHeight;
		}
		else {
			rectHeight = rectWidth * KinectWrapper.Constants.DepthImageHeight / KinectWrapper.Constants.DepthImageWidth;
		}
		Vector2 foregroundOfs = new Vector2((cameraRect.width - rectWidth) / 2, (cameraRect.height - rectHeight) / 2);
		return new Rect(foregroundOfs.x, cameraRect.height - foregroundOfs.y, rectWidth, -rectHeight);
	}
	

	void OnGUI()
	{
		// Draw live
		GUI.DrawTexture(liveRect, manager.GetUsersClrTex());
		if( mode == ManyMeMode.Rewind ) {
		GUI.DrawTexture(rewindRect, rewindTexture);
			if( thirdSelf ) {
				GUI.DrawTexture(rewindRect2, rewindTexture2);
			}
		} else if( mode == ManyMeMode.Snapshop ) {
			for( int i = 0; i < snapshots.Count; i++ ) {
				GUI.DrawTexture( snapshotRects[i], snapshots[i] );
			}
		}

		// Draw 
	}
}
