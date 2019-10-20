using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    float[][] main;
    private bool isVector;
    public int rows;
    public int columns;
    public Matrix(int rows, int columns){
        this.rows = rows;
        this.columns = columns;
        main = new float[rows][];
        for (int i = 0; i < rows; i++){
            main[i] = new float[columns];
        }
        if (columns == 1){ isVector = true; }
    }

    public static Matrix arrayToMatrix(float[][] orig){
        Matrix matrix = new Matrix(orig.Length, orig[0].Length);
        matrix.main = orig;

        return matrix;
    }

    public List<float> toList(){
        List<float> tmp = new List<float>();
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < columns; j++){
                tmp.Add(main[i][j]);
            }
        }

        return tmp;
    }

    public static Matrix toVector(List<float> list){
        Matrix mat = new Matrix(list.Count, 1);
        for (int i = 0; i < list.Count; i++){
            mat.insert(list[i], i);
        }
        return mat;
    }

    public void fillZeros(){
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < columns; j++){
                main[i][j] = 0.0f;
            }
        }
    }

    public void fillRandom(float min, float max){
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < columns; j++){
                main[i][j] = Random.Range(min, max);
            }
        }
    }

    public void appendRow(float[] newVals){
        if (newVals.Length != columns){
            Debug.Log("ERROR: Cannot append row with invalid number of columns");
            return;
        }

        List<float[]> tmpList = new List<float[]>();
        for (int i = 0; i < main.Length; i++){
            tmpList.Add(main[i]);
        }
        tmpList.Add(newVals);
        main = tmpList.ToArray();
    }

    public void insert(float val, int row, int col = 0){
        main[row][col] = val;
    }

    public float get(int row, int col = 0){
        return main[row][col];
    }

    public bool shapeEquals(Matrix otherMatrix){
        if (rows == otherMatrix.rows && columns == otherMatrix.columns){
            return true;
        }
        else {
            return false;
        }

    }


    public Matrix normalize(){
        List<float> list = this.toList();
        float sum = 0;
        for (int i = 0; i < list.Count; i++){
            sum += list[i] * list[i];
        }
        float len = Mathf.Sqrt(sum);
        for (int i = 0; i < list.Count; i++){
            list[i] = list[i]/len;
        }
        return Matrix.toVector(list);
    }

    public Matrix dot(Matrix otherMatrix){
        // if (isVector && otherMatrix.isVector && shapeEquals(otherMatrix)){
        //     vectorInnerProd(otherMatrix);
        // }

        if (columns != otherMatrix.rows){
            Debug.Log("ERROR: Invalid sized matrices for dot product");
            return this;
        }

        Matrix resultMat = new Matrix(rows, otherMatrix.rows);

        float currentSum = 0;

        for (int i = 0; i < rows; i++){ //My rows
            for (int j = 0; j < otherMatrix.columns; j++){ //Other cols
                for (int x = 0; x < columns; x++){ //My cols
                    currentSum += main[i][x] * otherMatrix.main[x][j];
                }
                currentSum = 0;
            }
        }

        return resultMat;

    }

    public float vectorInnerProd(Matrix otherMatrix){
        float res = 0;
        for (int i = 0; i < rows; i++){
            res += get(i) * otherMatrix.get(i);
        }

        return res;
    }



    public Matrix matrix_exp(){
        Matrix res = new Matrix(rows, columns);
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < columns; j++){
                res.main[i][j] = Mathf.Exp(main[i][j]);
            }
        }
        return res;
    }

    public Matrix sigmoid(){
        return 1.0f / (1.0f + matrix_exp());
    }

    public Matrix sigmoid_prime(){
        return this * (1 - this);
    }


    public void printMatrix(string tag = null){
        if (tag != null){
            Debug.Log("======== " + tag + " ========");
        }
        for (int i = 0; i < rows; i++){
            for (int j = 0; j < columns; j++){
                Debug.Log(main[i][j]);
            }
        }
        Debug.Log("============================");
    }

    public void printMatrixCell(int row, int col = 0){
        Debug.Log("Cell (" + row + ", " + col + "): " + main[row][col]);
    }

    public int vectorSize(){
        return rows;
    }

    // * ========== Overloaded operators ==========
    public static Matrix operator + (Matrix m1, Matrix m2)
    {
        // Debug.Log("M1: " + m1.vectorSize());
        // Debug.Log("M2: " + m2.vectorSize());
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] + m2.main[i][j];
            }
        }
        return m3;
    }

    public static Matrix operator + (Matrix m1, float val)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] + val;
            }
        }
        return m3;
    }
    public static Matrix operator + (float val, Matrix m1)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] + val;
            }
        }
        return m3;
    }

    public static Matrix operator - (Matrix m1, Matrix m2)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] - m2.main[i][j];
            }
        }
        return m3;
    }

    public static Matrix operator - (Matrix m1, float val)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] - val;
            }
        }
        return m3;
    }
     public static Matrix operator - (float val, Matrix m1)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = val - m1.main[i][j];
            }
        }
        return m3;
    }


    public static Matrix operator * (Matrix m1, Matrix m2)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] * m2.main[i][j];
            }
        }
        return m3;
    }

    public static Matrix operator * (Matrix m1, float val)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = m1.main[i][j] * val;
            }
        }
        return m3;
    }
     public static Matrix operator * (float val, Matrix m1)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                m3.main[i][j] = val * m1.main[i][j];
            }
        }
        return m3;
    }


    public static Matrix operator / (Matrix m1, Matrix m2)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                if (m1.main[i][j] == 0 || m2.main[i][j] == 0){
                    m3.main[i][j] = 0;
                    continue;
                }
                m3.main[i][j] = m1.main[i][j] / m2.main[i][j];

            }
        }
        return m3;
    }

    public static Matrix operator / (Matrix m1, float val)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                if (m1.main[i][j] == 0 || val == 0){
                    m3.main[i][j] = 0;
                    continue;
                }
                m3.main[i][j] = m1.main[i][j] / val;
            }
        }
        return m3;
    }
     public static Matrix operator / (float val, Matrix m1)
    {
        Matrix m3 = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++){
            for (int j = 0; j < m1.columns; j++){
                if (m1.main[i][j] == 0 || val == 0){
                    m3.main[i][j] = 0;
                    continue;
                }
                m3.main[i][j] = val / m1.main[i][j];
            }
        }
        return m3;
    }
}