/**
 * John P Beres
 * parse sic or sic xe program and output the object code along with the T records
 * from the command line type: sicxe.exe sic.txt 0; or: sicxe.exe xe.txt 1
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sicxe
{
    class Program
    {
        static void Main(string[] args)
        {
            //Begins the two pass assember and prints to file
            
            Assemble engageMrSulu = new Assemble(args[1].ToString(), args[0].ToString());
            engageMrSulu.PassOne();
            engageMrSulu.PassTwo();
            engageMrSulu.printData();
            engageMrSulu.printTRecord();
        }
    }

    class Assemble
    {
        //Variable and list declaration for storing opcodes and the symbol table
        int sicorxe;
        int currLocation;
        int opCodeCheck;
        static String fileName;
        List<symbolTable> symTab = new List<symbolTable>();
        constantLibrary conTab = new constantLibrary();
        List<String> memCounter = new List<String>();
        List<String> objCode = new List<String>();
        SourceFile sFile;
        String baseDeclaration;

        public Assemble(String i, String a)
        {
            fileName = a;
            sFile = new SourceFile(fileName);
            this.sicorxe = Int32.Parse(i);
        }

        public void PassOne()
        {
            int lineCounter = 0;
            String tempString;
            char[] remove = {'\'', 'C' , 'X'};
            currLocation = Int32.Parse(sFile.getOperand(0), System.Globalization.NumberStyles.HexNumber);

            //The first to lines of the file have the same memory location so intialize them
            memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
            memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
            symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
            lineCounter++;
            symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
            lineCounter++;
            currLocation += 3;

            //Go through each line of the source code and set the memory displacment
            for (; lineCounter < sFile.getSize() - 1; lineCounter++)
            {
                //Adds a new symbol to the symbol table
                tempString = sFile.getSouceCode(lineCounter);
                if (symTableLookUp(sFile.getLabel(lineCounter)) == -1 && sFile.getLabel(lineCounter).Length >= 2)
                {
                    symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
                }
                
                //if its sic use this memory displacement
                if (sicorxe == 0)
                {
                    if (-1 != (opCodeCheck = conTab.findOpCode(tempString.Trim('+'))))
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 3;
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESB") == 0)
                    {
                        currLocation += Int32.Parse(sFile.getOperand(lineCounter));
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("BYTE") == 0)
                    {

                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        if (sFile.getOperand(lineCounter).StartsWith("C"))
                        {
                            currLocation += System.Text.ASCIIEncoding.UTF8.GetByteCount(tempString.Trim(remove)) - 1;
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("X"))
                        {
                            currLocation += 1;
                        }
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("WORD") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 3;
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESW") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 3 * Int32.Parse(sFile.getOperand(lineCounter));
                    }  
                    
                }
                // if its xe use this memory displacment
                else
                {
                    if (sFile.getSouceCode(lineCounter).CompareTo("BASE") == 0)
                    {
                        currLocation += 0;
                        baseDeclaration = sFile.getOperand(lineCounter);
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESB") == 0)
                    {

                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += Int32.Parse(sFile.getOperand(lineCounter));
                        
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("BYTE") == 0)
                    {

                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        if (sFile.getOperand(lineCounter).StartsWith("C"))
                        {
                            currLocation += System.Text.ASCIIEncoding.UTF8.GetByteCount(tempString.Trim(remove)) - 1;
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("X"))
                        {
                            currLocation += 1;
                        }
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("WORD") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 3;
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESW") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 3 * Int32.Parse(sFile.getOperand(lineCounter));
                    }
                    else if (sFile.getSouceCode(lineCounter).StartsWith("+"))
                    {
                        tempString = sFile.getSouceCode(lineCounter);
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += 4;
                        opCodeCheck = conTab.findOpCode(tempString.Trim('+'));
                        
                    }
                    else if (-1 != (opCodeCheck = conTab.findOpCode(tempString.Trim('+'))))
                    {
                        memCounter.Add(currLocation.ToString("X").PadLeft(6, '0'));
                        currLocation += (conTab.getMemAddressMode(sFile.getSouceCode(lineCounter)));
                    }
                }
            }
        }

        public void PassTwo()
        {
            String tempString = "";
            char[] remove = { '\'', 'C', 'X' };
            int baseAddess = 0;
            int pcCounter = 0;
            int taAddress = 0;
            int address = 0;
            int lineCounter = 1;
            objCode.Add("0");
            //if its xe get the base address for calculating displacement
            if (sicorxe != 0)
            {
                baseAddess = Int32.Parse(symTab[symTableLookUp(baseDeclaration)].getLocation());
            }

            for (; lineCounter < sFile.getSize() - 1; lineCounter++)
            {
                byte[] tempByteArray = { 0 };
                int tempAddress = 0;
                int sourceCode = 0;
                tempString = "";
                address = 0;

                //make sure you dont grab an non existant forward memory address
                if (sFile.getSouceCode(lineCounter + 1).CompareTo("END") != 0 && sFile.getSouceCode(lineCounter + 1).CompareTo("NULL") != 0)
                {
                    pcCounter = Int32.Parse(memCounter[lineCounter + 1], System.Globalization.NumberStyles.HexNumber);
                }
                //dont bother calculacting object code for base resw and resb
                if (sFile.getSouceCode(lineCounter).CompareTo("BASE") != 0 && sFile.getSouceCode(lineCounter).CompareTo("RESW") != 0 && sFile.getSouceCode(lineCounter).CompareTo("RESB") != 0)
                {
                    //byte and word dont change regardless of sic or xe do this first
                    if (sFile.getSouceCode(lineCounter).CompareTo("BYTE") == 0)
                    {
                        if (sFile.getOperand(lineCounter).StartsWith("C"))
                        {
                            tempByteArray = Encoding.ASCII.GetBytes(sFile.getOperand(lineCounter).Trim(remove));
                            for (int i = 0; i < tempByteArray.Length; i++)
                            {
                                tempString = tempString + tempByteArray[i].ToString("X");
                            }
                            objCode.Add(tempString);
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("X"))
                        {
                            objCode.Add(sFile.getOperand(lineCounter).Trim(remove));
                        }
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("WORD") == 0)
                    {
                        if (sicorxe == 0)
                        {
                            objCode.Add(Int32.Parse(sFile.getOperand(lineCounter)).ToString("X").PadLeft(6, '0'));
                        }
                        else
                        {
                            objCode.Add(Int32.Parse(sFile.getOperand(lineCounter)).ToString("X"));
                        }
                    }
                    //Memory addess mode 4
                    else if (sFile.getSouceCode(lineCounter).StartsWith("+"))
                    {
                        sourceCode = conTab.getOpCode(sFile.getSouceCode(lineCounter));
                        

                        if (sFile.getOperand(lineCounter).StartsWith("#"))
                        {
                            tempAddress = Int32.Parse(sFile.getOperand(lineCounter).Trim('#'));
                            sourceCode += 1;

                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("@"))
                        {
                            tempAddress = getLabelLocation(sFile.getOperand(lineCounter));
                            sourceCode += 2;
                        }
                        else
                        {
                            tempAddress = getLabelLocation(sFile.getOperand(lineCounter));
                            sourceCode += 3;
                        }
                        tempAddress += 1048576;
                        objCode.Add(sourceCode.ToString("X").PadLeft(2,'0') + tempAddress.ToString("X").PadLeft(6, '0'));
                    }
                    //If there is no operand the same regardless of sic or xe
                    else if (sFile.getOperand(lineCounter) == " ")
                    {
                        sourceCode = conTab.getOpCode(sFile.getSouceCode(lineCounter));
                        if (sicorxe != 0)
                        {
                            sourceCode += 3;
                        }
                        objCode.Add(sourceCode.ToString("X").PadRight(6, '0'));
                    }
                    // if its sic
                    else if (sicorxe == 0)
                    {
                        tempAddress = getLabelLocation(sFile.getOperand(lineCounter));
                        sourceCode = conTab.getOpCode(sFile.getSouceCode(lineCounter));
                        if (sFile.getOperand(lineCounter).EndsWith("X"))
                        {
                            tempAddress += 32768;
                        }
                        objCode.Add(sourceCode.ToString("X").PadLeft(2, '0') + tempAddress.ToString("X").PadLeft(4, '0'));
                    }
                    //memory address mode 1
                    else if (sicorxe != 0 && conTab.getMemAddressMode(sFile.getSouceCode(lineCounter)) == 1)
                    {
                        objCode.Add(conTab.getOpCode(sFile.getSouceCode(lineCounter)).ToString("X"));
                    }
                    //memory address mode 2
                    else if (sicorxe != 0 && conTab.getMemAddressMode(sFile.getSouceCode(lineCounter)) == 2)
                    {
                        int temp = 0;
                        sourceCode = conTab.getOpCode(sFile.getSouceCode(lineCounter));
                        tempAddress = conTab.getRegisterValue(sFile.getOperand(lineCounter).Substring(0, 1));

                        if (sFile.getOperand(lineCounter).Length >= 2)
                        {
                            temp = conTab.getRegisterValue(sFile.getOperand(lineCounter).Substring(sFile.getOperand(lineCounter).Length - 1, 1));
                            objCode.Add(sourceCode.ToString("X").PadLeft(2, '0') + tempAddress.ToString("X") + temp.ToString("X"));
                        }
                        else
                        {
                            objCode.Add(sourceCode.ToString("X").PadLeft(2, '0') + tempAddress.ToString("X").PadRight(2, '0'));
                        }
                    }
                    //memory address mode 3
                    // used the decimal equivalent to tripping a specific bit
                    else if (sicorxe != 0 && conTab.getMemAddressMode(sFile.getSouceCode(lineCounter)) == 3)
                    {
                        sourceCode = conTab.getOpCode(sFile.getSouceCode(lineCounter));

                        if (isInTable(sFile.getOperand(lineCounter)))
                        {
                            taAddress = Int32.Parse(symTab[symTableLookUp(sFile.getOperand(lineCounter))].getLocation(), System.Globalization.NumberStyles.HexNumber);
                            if (-2048 <= (taAddress - pcCounter) && (taAddress - pcCounter) <= 2047)
                            {
                                address = taAddress - pcCounter;
                                if (address < 0)
                                {
                                    address += 4096;
                                }
                                address += 8192;
                            }
                            else if (0 >= (taAddress - baseAddess) && (taAddress - baseAddess) <= 4096)
                            {
                                address = taAddress - baseAddess;
                                address += 16384;
                            }
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("#"))
                        {
                            tempString = sFile.getOperand(lineCounter);
                            tempString = tempString.Trim('#');
                            address = Int32.Parse(tempString);
                        }

                        if (sFile.getOperand(lineCounter).StartsWith("#"))
                        {
                            sourceCode += 1;
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("@"))
                        {
                            sourceCode += 2;
                        }
                        else
                        {
                            sourceCode += 3;
                        }

                        if (sFile.getOperand(lineCounter).EndsWith("X"))
                        {
                            address += 32768;
                        }

                        objCode.Add(sourceCode.ToString("X").PadLeft(2, '0') + address.ToString("X").PadRight(4, '0'));
                    }
                }
                else
                {
                    objCode.Add("NULL");
                }
            }
        }

        //check if its in the table
        private bool isInTable(String search)
        {
            if(symTableLookUp(search) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //gets the location of the label and returns it as an integer
        private int getLabelLocation(String search)
        {

            int fetch = Int32.Parse(symTab[symTableLookUp(search)].getLocation(), System.Globalization.NumberStyles.HexNumber);
            return fetch;
        }

        // gets the location of the label in the symbol table
        private int symTableLookUp(String search)
        {
            for(int i = 0; i < symTab.Count; i++)
            {
                if(search.Contains(symTab[i].getLabelName()))
                {
                    return i;
                }
            }
            return -1;
        }

        //Prints the source code out to a file
        public void printData()
        {
            int commentCounter = 0;
            int i = 0;
            TextWriter output;
            if(sicorxe == 0)
            {
                output = new StreamWriter("sicoutput.txt");
                for(i = 0; i < objCode.Count; i++)
                {
                    if(sFile.getSouceCode(i).CompareTo("BASE") != 0 && sFile.getSouceCode(i).CompareTo("END") != 0)
                    {
                        output.Write(memCounter[i].ToString().PadRight(10, ' '));
                    }
                    else
                    {
                        output.Write(" ".PadRight(10, ' '));
                    }
                    output.Write(sFile.getLabel(i).PadRight(10, ' ') + sFile.getSouceCode(i).PadRight(10, ' ') + sFile.getOperand(i).PadRight(10, ' '));
                    if (sFile.getSouceCode(i).CompareTo("BASE") != 0 && sFile.getSouceCode(i).CompareTo("END") != 0 && sFile.getSouceCode(i).CompareTo("RESB") != 0 && sFile.getSouceCode(i).CompareTo("RESW") != 0 && sFile.getSouceCode(i).CompareTo("START") != 0)
                    {
                        output.Write(objCode[i].ToString());
                    }
                    output.Write("\n");
                    if (commentCounter < sFile.getCommentSize() && sFile.getSouceCode(i).CompareTo(sFile.getCommentLocation(commentCounter)) == 0)
                    {
                        do
                        {
                            output.WriteLine(sFile.getComment(commentCounter));
                            commentCounter++;
                        }
                        while (commentCounter < sFile.getCommentSize() && sFile.getSouceCode(i).CompareTo(sFile.getCommentLocation(commentCounter)) == 0);
                    }
                }
                output.Write(sFile.getLabel(i).PadRight(20, ' ') + sFile.getSouceCode(i).PadRight(10, ' ') + sFile.getOperand(i).PadRight(10, ' '));
                output.Close();
            }
            else
            {
                output = new StreamWriter("xeoutput.txt");
                for( i = 0; i < objCode.Count; i++)
                {
                    if (sFile.getSouceCode(i).CompareTo("BASE") != 0 && sFile.getSouceCode(i).CompareTo("END") != 0)
                    {
                        output.Write(memCounter[i].ToString().PadRight(10, ' '));
                    }
                    else
                    {
                        output.Write(" ".PadRight(10, ' '));
                    }
                    output.Write(sFile.getLabel(i).PadRight(10, ' ') + sFile.getSouceCode(i).PadRight(10, ' ') + sFile.getOperand(i).PadRight(10, ' '));
                    if (sFile.getSouceCode(i).CompareTo("BASE") != 0 && sFile.getSouceCode(i).CompareTo("END") != 0 && sFile.getSouceCode(i).CompareTo("RESB") != 0 && sFile.getSouceCode(i).CompareTo("RESW") != 0 && sFile.getSouceCode(i).CompareTo("START") != 0)
                    {
                        output.Write(objCode[i].ToString());
                    }
                    output.Write("\n");
                    if (commentCounter < sFile.getCommentSize() && sFile.getSouceCode(i).CompareTo(sFile.getCommentLocation(commentCounter)) == 0)
                    {
                        do
                        {
                            output.WriteLine(sFile.getComment(commentCounter));
                            commentCounter++;
                        }
                        while (commentCounter < sFile.getCommentSize() && sFile.getSouceCode(i).CompareTo(sFile.getCommentLocation(commentCounter)) == 0);
                    }

                }
                output.Write(sFile.getLabel(i).PadRight(20, ' ') + sFile.getSouceCode(i).PadRight(10, ' ') + sFile.getOperand(i).PadRight(10, ' '));
                output.Close();
            }

        }

        //prints out the T records
        public void printTRecord()
        {
            String tempString = "";
            String TBuilder = "";
            int counter = 0;
            int tempStorage = 0;

            tempStorage = Int32.Parse(memCounter[0], System.Globalization.NumberStyles.HexNumber);
            tempStorage = (Int32.Parse(memCounter[memCounter.Count - 1], System.Globalization.NumberStyles.HexNumber) - tempStorage) + 1;

            //First line is always the same
            tempString = "H" + sFile.getLabel(0) + "\t\t" + memCounter[0] + tempStorage.ToString("X");

            TextWriter output;
            if (sicorxe == 0)
            {
                output = new StreamWriter("sicTREC.txt");
            }
            else
            {
                output = new StreamWriter("xeTREC.txt");
            }

            output.WriteLine(tempString);

            tempString = "";
            tempStorage = 0;

            //when you hit a break in the object code print out what is already built then start again
            for (int i = 1; i < objCode.Count; i++)
            {
                if (counter == 0 && objCode[i].CompareTo("NULL") != 0)
                {
                    TBuilder = "T" + memCounter[i];
                }
                if (objCode[i].CompareTo("NULL") != 0 && (objCode[i].Count() + counter) <= 60)
                {
                    tempString = tempString + objCode[i];
                    counter += objCode[i].Count();
                }
                else
                {
                    if (objCode[i].CompareTo("NULL") == 0 && counter > 0 && sFile.getSouceCode(i).CompareTo("BASE") != 0)
                    {
                        TBuilder = TBuilder + (counter / 2).ToString("X").PadLeft(2, '0') + tempString;
                        tempString = "";
                        counter = 0;
                        output.WriteLine(TBuilder);
                    }
                    else if (objCode[i].CompareTo("NULL") != 0 && (objCode[i].Count() + counter) > 60)
                    {
                        TBuilder = TBuilder + (counter/2).ToString("X").PadLeft(2,'0') + tempString;
                        tempString = "";
                        counter = 0;
                        output.WriteLine(TBuilder);
                        TBuilder = "T" + memCounter[i];
                        tempString = tempString + objCode[i];
                        counter += objCode[i].Count();
                    }

                }
            }

            //prints out the remaining T record and the E record
            TBuilder = TBuilder + (counter / 2).ToString("X").PadLeft(2, '0') + tempString;
            output.WriteLine(TBuilder);
            output.WriteLine("E" + memCounter[0]);
            output.Close();
        }
    }
    
    /**
     * create an object for manipulating the symbol table
     */
    class symbolTable
    {
        private String labelName;
        private String location;
       
        public symbolTable(String a, String b)
        {
            this.labelName = a;
            this.location = b;
        }

        public String getLabelName()
        {
            return labelName;
        }

        public String getLocation()
        {
            return location;
        }

        public bool Equals(symbolTable other)
        {
            if (this.labelName.CompareTo(other.labelName) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(symbolTable other)
        {
            return this.labelName.CompareTo(other.labelName);
        }
    }

    /**
     * create an object to better manipulate the file once read into the program
     */
    class FileFormat
    {
        private String label;
        private String sourceCode;
        private String operand;

        public FileFormat(String a, String b, String c)
        {
            this.label = a;
            this.sourceCode = b;
            this.operand = c;
        }

        public String getLabel()
        {
            return label;
        }

        public String getSouceCode()
        {
            return sourceCode;
        }

        public String getOperand()
        {
            return operand;
        }
    }
    
    /**
     * creates an object for manipulating the comments as they are parsed into a seprate list
     */
    class SourceComments
    {
        private String sourceCode;
        private String comment;

        public SourceComments(String a, String b)
        {
            sourceCode = a;
            comment = b;
        }

        public String getSourceCode()
        {
            return sourceCode;
        }

        public String getComment()
        {
            return comment;
        }
    }


    /**
     * builds the source file from the .txt file specified and breakes each line into columns
     * and puts the comments into their own container for later
     */
    class SourceFile
    {
        List<FileFormat> buildSourceCode = new List<FileFormat>();
        String tempLine;
        String label;
        String sourceCode;
        String operand;
        String[] stringParse;
        int size = 0;
        List<SourceComments> comments = new List<SourceComments>();

        public SourceFile(String fileName)
        {
            TextReader fileInput = File.OpenText(fileName);
            int sCodeCheck = 0;
            while (fileInput.Peek() > -1)
            {

                tempLine = fileInput.ReadLine();

                if (!tempLine.StartsWith("."))
                {
                    size++;

                    stringParse = tempLine.Split(' ');

                    label = stringParse[0];
                    
                    for (int i = 1; i < stringParse.Length; i++)
                    {
                        if (stringParse[i].Length >= 1)
                        {
                            sCodeCheck = i;
                            sourceCode = stringParse[i];
                            if (stringParse.Last().CompareTo(sourceCode) == 0)
                            {
                                operand = " ";
                                break;
                            }
                            else
                            {
                                operand = stringParse.Last();
                                break;
                            }
                        }
                    }
                    buildSourceCode.Add(new FileFormat(label, sourceCode, operand));
                }
                else if ((tempLine.StartsWith(".")))
                {
                    //Every line has a sourcecode so its a good way of determineing whan a section of comments starts and ends
                    comments.Add(new SourceComments(sourceCode, tempLine));
                }

            }
        }

        /**
         * get methods for the two lists that are created
         */
        public int getSize()
        {
            return size;
        }

        public String getLabel(int i)
        {
            return buildSourceCode[i].getLabel();
        }

        public String getSouceCode(int i)
        {
            return buildSourceCode[i].getSouceCode();
        }

        public String getOperand(int i)
        {
            return buildSourceCode[i].getOperand();
        }

        public String toString(int i)
        {
            return (buildSourceCode[i].getLabel() + " " + buildSourceCode[i].getSouceCode() + " " + buildSourceCode[i].getOperand());
        }

        public int getCommentSize()
        {
            return comments.Count;
        }

        public String getCommentLocation(int i)
        {
            return comments[i].getSourceCode();
        }

        public String getComment(int i)
        {
            return comments[i].getComment();
        }
    }

    /**
     * create class for manipulating the opcode table
     */
    class opcodeTable
    {
        private String sourceCode;
        private String opCode;
        private String type;

        public opcodeTable(String a, String b, String c)
        {
            this.sourceCode = a;
            this.opCode = b;
            this.type = c;
        }

        public String getSourceCode()
        {
            return sourceCode;
        }

        public String getOpCode()
        {
            return opCode;
        }

        public String getType()
        {
            return type;
        }

        public bool Equals(opcodeTable other)
        {
            if (this.sourceCode.CompareTo(other.sourceCode) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(opcodeTable other)
        {
            return this.sourceCode.CompareTo(other.sourceCode);
        }
    }

    /**
     * class for manipulating the register table
     */
    class registerTable
    {
        private String register;
        private String value;

        public registerTable(String a, String b)
        {
            this.register = a;
            this.value = b;
        }

        public String getRegister()
        {
            return register;
        }

        public String getValue()
        {
            return value;
        }

        public bool Equals(registerTable other)
        {
            if (this.register.CompareTo(other.register) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(registerTable other)
        {
            return this.register.CompareTo(other.register);
        }
    }

    /**
     * take the opcodetable class and registertable class and build a library for accesing those tables
     */
    class constantLibrary
    {
        List<opcodeTable> opTable = new List<opcodeTable>();
        List<registerTable> regTable = new List<registerTable>();

        public constantLibrary()
        {
            opTable.Add(new opcodeTable("ADD", "18", "3"));   opTable.Add(new opcodeTable("ADDR", "90", "2"));   opTable.Add(new opcodeTable("AND", "40", "3"));
            opTable.Add(new opcodeTable("CLEAR", "B4", "2")); opTable.Add(new opcodeTable("COMP", "28", "3"));   opTable.Add(new opcodeTable("COMPR", "A0", "2"));
            opTable.Add(new opcodeTable("DIV", "24", "3"));   opTable.Add(new opcodeTable("DIVR", "9C", "2"));   opTable.Add(new opcodeTable("HIO", "F4", "1"));
            opTable.Add(new opcodeTable("J", "3C", "3"));     opTable.Add(new opcodeTable("JEQ", "30", "3"));    opTable.Add(new opcodeTable("JGT", "34", "3"));
            opTable.Add(new opcodeTable("JLT", "38", "3"));   opTable.Add(new opcodeTable("JSUB", "48", "3"));   opTable.Add(new opcodeTable("LDA", "00", "3"));
            opTable.Add(new opcodeTable("LDB", "68", "3"));   opTable.Add(new opcodeTable("LDCH", "50", "3"));   opTable.Add(new opcodeTable("LDL", "08", "3"));
            opTable.Add(new opcodeTable("LDS", "6C", "3"));   opTable.Add(new opcodeTable("LDT", "74", "3"));    opTable.Add(new opcodeTable("LDX", "04", "3"));
            opTable.Add(new opcodeTable("LPS", "D0", "3"));   opTable.Add(new opcodeTable("MUL", "20", "3"));    opTable.Add(new opcodeTable("MULR", "98", "2"));
            opTable.Add(new opcodeTable("OR", "44", "3"));    opTable.Add(new opcodeTable("RD", "D8", "3"));     opTable.Add(new opcodeTable("RMO", "AC", "2"));
            opTable.Add(new opcodeTable("RSUB", "4C", "3"));  opTable.Add(new opcodeTable("SHIFTL", "A4", "2")); opTable.Add(new opcodeTable("SHIFTR", "A8", "2"));
            opTable.Add(new opcodeTable("SIO", "F0", "1"));   opTable.Add(new opcodeTable("SSK", "EC", "3"));    opTable.Add(new opcodeTable("STA", "0C", "3"));
            opTable.Add(new opcodeTable("STB", "78", "3"));   opTable.Add(new opcodeTable("STCH", "54", "3"));   opTable.Add(new opcodeTable("STI", "D4", "3"));
            opTable.Add(new opcodeTable("STL", "14", "3"));   opTable.Add(new opcodeTable("STS", "7C", "3"));    opTable.Add(new opcodeTable("STSW", "E8", "3"));
            opTable.Add(new opcodeTable("STT", "84", "3"));   opTable.Add(new opcodeTable("STX", "10", "3"));    opTable.Add(new opcodeTable("SUB", "1C", "3"));
            opTable.Add(new opcodeTable("SUBR", "94", "2"));  opTable.Add(new opcodeTable("SVC", "B0", "2"));    opTable.Add(new opcodeTable("TD", "E0", "3"));
            opTable.Add(new opcodeTable("TIO", "F8", "1"));   opTable.Add(new opcodeTable("TIX", "2C", "3"));    opTable.Add(new opcodeTable("TIXR", "B8", "2"));
            opTable.Add(new opcodeTable("WD", "DC", "3"));    opTable.Add(new opcodeTable("ADDF", "58", "3"));   opTable.Add(new opcodeTable("COMPF", "88", "3"));
            opTable.Add(new opcodeTable("DIVF", "64", "3"));  opTable.Add(new opcodeTable("FIX", "C4", "1"));    opTable.Add(new opcodeTable("FLOAT", "C0", "1"));
            opTable.Add(new opcodeTable("LDF", "70", "3"));   opTable.Add(new opcodeTable("MULF", "60", "3"));   opTable.Add(new opcodeTable("NORM", "C8", "1"));
            opTable.Add(new opcodeTable("STF", "80", "3"));   opTable.Add(new opcodeTable("SUBF", "5C", "3"));

            regTable.Add(new registerTable("A", "0"));
            regTable.Add(new registerTable("X", "1"));
            regTable.Add(new registerTable("L", "2"));
            regTable.Add(new registerTable("PC", "8"));
            regTable.Add(new registerTable("SW", "9"));
            regTable.Add(new registerTable("B", "3"));
            regTable.Add(new registerTable("S", "4"));
            regTable.Add(new registerTable("T", "5"));
            regTable.Add(new registerTable("F", "6"));
        }

        public int findOpCode(String search)
        {
            search.Trim('+');
            opcodeTable searchOpCodes = new opcodeTable(search, "", "");
            for (int i = 0; i < opTable.Count; i++)
            {
                if (search.CompareTo(opTable[i].getSourceCode()) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool isOpCode(String search)
        {
            if (findOpCode(search) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int getOpCode(String search)
        {
            
            return Int32.Parse(opTable[findOpCode(search.Trim('+'))].getOpCode(),System.Globalization.NumberStyles.HexNumber);
        }

        public int getMemAddressMode(String search)
        {
            return Int32.Parse(opTable[findOpCode(search)].getType());
        }

        public int findRegister(String search)
        {
            registerTable searchRegisters = new registerTable(search, "");
            for (int j = 0; j < regTable.Count; j++)
            {
                if (search.Contains(regTable[j].getRegister()))
                {
                    return j;
                }
            }
            return -1;
        }
        public bool isRegister(String search)
        {
            if (findOpCode(search) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
            
        public int getRegisterValue(String search)
        {
            return Int32.Parse(regTable[findRegister(search)].getValue());
        }

    }
}
