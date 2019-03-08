using UnityEngine;
using System.Collections;

public class TreeSystem : MonoBehaviour {
	
	public GameObject planet;
	public GameObject cam;
	public float radius;
    public int numberOfTrees = 500;
	System.Random randGen;

    GameObject tree, tree2, tree3;
    GameObject foliage, bush;
	Component[] treeComponents;
	// Use this for initialization
	void Start () {
		randGen = new System.Random(100500);
		tree = GameObject.Find("Tree");
        treeComponents = tree.GetComponents(typeof(Component));
		for(int i=0; i< numberOfTrees; i++)SetTree(i, tree);


        tree2 = GameObject.Find("Tree2");
        treeComponents = tree2.GetComponents(typeof(Component));
        for (int i = 0; i < numberOfTrees; i++) SetTree(i, tree2);

        tree3 = GameObject.Find("Tree3");
        treeComponents = tree3.GetComponents(typeof(Component));
        for (int i = 0; i < numberOfTrees; i++) SetTree(i, tree3);

        foliage = GameObject.Find("Foliage");
        treeComponents = foliage.GetComponents(typeof(Component));
        for (int i = 0; i < numberOfTrees; i++) SetTree(i, foliage);

        bush = GameObject.Find("bush");
        treeComponents = bush.GetComponents(typeof(Component));
        for (int i = 0; i < numberOfTrees; i++) SetTree(i, bush);
    }
	
	
	void SetTree(int number, GameObject CurTree){
		Vector3 pos = Random.insideUnitSphere;
		pos.Normalize();
		float height = MagicData.Height(pos);//two.Height(pos);	
		if(height>1){
		GameObject currentTree = (GameObject) Instantiate(CurTree);
		currentTree.transform.parent = gameObject.transform;		
		currentTree.transform.position = pos * (radius) * height ;	
		currentTree.transform.up = pos;
        currentTree.transform.RotateAroundLocal(pos, Random.value*360);
            Vector3 scale = new Vector3((0.5f + Random.value*0.5f), (0.5f + Random.value * 0.5f), (0.5f + Random.value * 0.5f));
        currentTree.transform.localScale.Scale(scale);

        }
		
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
}
