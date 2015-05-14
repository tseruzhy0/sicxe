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
            Console.WriteLine(args[0] + " " + args[1]);
            
            byte[] temp2;
            //Assemble engageMrSulu = new Assemble(args[1].ToString(), args[0].ToString());
            //engageMrSulu.PassOne();

            String temp = "EOF";
            temp2 = Encoding.ASCII.GetBytes(temp);
            temp = "";
            for (int i = 0; i < temp2.Length; i++)
            {
                Console.WriteLine(temp2.Length);
                Console.WriteLine( temp = temp + temp2[i].ToString("X"));
            }
            //temp = Int32.Parse(temp, System.Globalization.NumberStyles.HexNumber).ToString();
            
            Console.WriteLine(temp);


        }
    }

    class Assemble
    {
        int sicorxe;
        int currLocation;
        int opCodeCheck;
        static String fileName;
        List<symbolTable> symTab = new List<symbolTable>();
        constantLibrary conTab = new constantLibrary();
        List<String> memCounter = new List<String>();
        List<String> objCode = new List<String>();
        SourceFile sFile;
        int baseDeclaration;

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

            memCounter.Add(currLocation.ToString("X"));
            memCounter.Add(currLocation.ToString("X"));
            symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
            lineCounter++;
            symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
            lineCounter++;
            currLocation += 3;

            

            for (; lineCounter < sFile.getSize() - 1; lineCounter++)
            {
                tempString = sFile.getSouceCode(lineCounter);
                if (symTableLookUp(sFile.getLabel(lineCounter)) == -1 && sFile.getLabel(lineCounter).Length >= 2)
                {
                    symTab.Add(new symbolTable(sFile.getLabel(lineCounter), currLocation.ToString("X")));
                }
    
                if (sicorxe == 0)
                {
                    if (-1 != (opCodeCheck = conTab.findOpCode(tempString.Trim('+'))))
                    {
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 3;
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESB") == 0)
                    {
                        currLocation += Int32.Parse(sFile.getOperand(lineCounter));
                        memCounter.Add(currLocation.ToString("X"));
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("BYTE") == 0)
                    {
                        
                        memCounter.Add(currLocation.ToString("X"));
                        if (sFile.getOperand(lineCounter).StartsWith("C"))
                        {
                            currLocation += System.Text.ASCIIEncoding.UTF8.GetByteCount(tempString.Trim(remove));
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("X"))
                        {
                            currLocation += 1;
                        }
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("WORD") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 3;
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESW") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 3 * Int32.Parse(sFile.getOperand(lineCounter));
                    }  
                    
                }
                else
                {
                    if (sFile.getSouceCode(lineCounter).CompareTo("BASE") == 0)
                    {
                        currLocation += 0;
                        baseDeclaration = lineCounter;
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESB") == 0)
                    {
                        
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += Int32.Parse(sFile.getOperand(lineCounter));
                        
                        
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("BYTE") == 0)
                    {
                        
                        memCounter.Add(currLocation.ToString("X"));
                        if (sFile.getOperand(lineCounter).StartsWith("C"))
                        {
                            currLocation += System.Text.ASCIIEncoding.UTF8.GetByteCount(tempString.Trim(remove));
                        }
                        else if (sFile.getOperand(lineCounter).StartsWith("X"))
                        {
                            currLocation += 1;
                        }
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("WORD") == 0)
                    {
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 3;
                    }
                    else if (sFile.getSouceCode(lineCounter).CompareTo("RESW") == 0)
                    {
                        
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 3 * Int32.Parse(sFile.getOperand(lineCounter));

                    }
                    else if (sFile.getSouceCode(lineCounter).StartsWith("+"))
                    {
                        tempString = sFile.getSouceCode(lineCounter);
                        
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += 1;

                        opCodeCheck = conTab.findOpCode(tempString.Trim('+'));
                        if (-1 != opCodeCheck)
                        {
                            currLocation += Int32.Parse(conTab.getMemAddressMode(opCodeCheck));
                        }
                        
                    }
                    else if (-1 != (opCodeCheck = conTab.findOpCode(tempString.Trim('+'))))
                    {
                        
                        memCounter.Add(currLocation.ToString("X"));
                        currLocation += Int32.Parse(conTab.getMemAddressMode(opCodeCheck));

                    }
                }
            }
        }

        public void PassTwo()
        {
            String tempString;
            String opcode;
            String operand;
            String addressMode = " ";
            String curMemLocation;
            String labelMemLocation = " ";
            Boolean operandHasLabel = false;
            int opCodeCheck;
            int baseAddess = Int32.Parse(symTab[symTableLookUp(sFile.getOperand(baseDeclaration))].getLocation(),System.Globalization.NumberStyles.HexNumber);
            int pcCounter;
            int targetAddress;
            int newAddress;
            int tempVal;
            char[] remove = { '\'', 'C', 'X' };
            byte[] tempByte;
            int lineCounter = 1;

            for (; lineCounter < sFile.getSize() - 1; lineCounter++)
            {
                opcode = sFile.getSouceCode(lineCounter);
                operand = sFile.getOperand(lineCounter);
                curMemLocation = memCounter[lineCounter];
                tempString = opcode;
                tempString.Trim('+');
                opCodeCheck = conTab.findOpCode(tempString);
                pcCounter = Int32.Parse(memCounter[lineCounter + 1], System.Globalization.NumberStyles.HexNumber);
                if (opCodeCheck != -1)
                {
                    addressMode = conTab.getMemAddressMode(opCodeCheck);
                }
                if (symTableLookUp(operand) != -1)
                {
                    operandHasLabel = true;
                    labelMemLocation = symTab[symTableLookUp(operand)].getLocation();
                }



                if (opcode.CompareTo("RESB") != 0 && opcode.CompareTo("BASE") != 0 && opcode.CompareTo("RESW") != 0)
                {
                    if (opcode.CompareTo("WORD") == 0)
                    {
                        tempString = operand;
                        if (sicorxe == 0)
                        {
                            tempString.PadLeft(6, '0');
                        }
                        objCode.Add(tempString);
                    }
                    else if (opcode.CompareTo("BYTE") == 0)
                    {
                        if (operand.StartsWith("C"))
                        {
                            tempString = operand;
                            tempString.Trim(remove);
                            tempByte = Encoding.ASCII.GetBytes(tempString);
                            tempString = " ";
                            for (int i = 0; i < tempByte.Length; i++)
                            {
                                tempString = tempString + tempByte[i].ToString("X");
                            }
                            objCode.Add(tempString);
                        }
                        else if (operand.StartsWith("X"))
                        {
                            tempString.Trim(remove);
                            objCode.Add(tempString);
                        }
                        else if (opCodeCheck != -1)
                        {
                            if (sicorxe == 0)
                            {
                                if (operandHasLabel)
                                {
                                    if (operand.EndsWith("X"))
                                    {
                                        tempVal = Int32.Parse(labelMemLocation, System.Globalization.NumberStyles.HexNumber);
                                        tempVal += -32768;
                                        tempString = tempVal.ToString("X");
                                        tempString = conTab.getOpCode(conTab.findOpCode(opcode)) + tempString;
                                        objCode.Add(tempString);
                                    }
                                    else
                                    {
                                        tempString = conTab.getOpCode(conTab.findOpCode(opcode)) + labelMemLocation.PadLeft(4, '0');
                                        objCode.Add(tempString);
                                    }
                                }
                            }
                            else
                            {
                                const int nBit = 2;
                                const int iBit = 1;
                                const int xBit = 8;
                                const int bBit = 4;
                                const int pBit = 2;
                                const int eBit = 1;

                                
                                if (opcode.StartsWith("+"))
                                {
                                    if (operand.StartsWith("#"))
                                    {
                                        tempString = Int32.Parse(operand.Trim('#'), System.Globalization.NumberStyles.HexNumber).ToString().PadLeft(5, '0');
                                        objCode.Add(addModeFour(opcode, iBit, eBit, tempString));
                                    }
                                    else
                                    {
                                        objCode.Add(addModeFour(opcode, (nBit + iBit), eBit, symTab[symTableLookUp(operand)].getLocation().PadLeft(5, '0')));
                                    }
                                }
                                else if (addressMode.CompareTo("1") == 0)
                                {
                                    objCode.Add(conTab.getOpCode(conTab.findOpCode(opcode)).PadLeft(2, '0'));
                                }
                                else if (addressMode.CompareTo("2") == 0)
                                {
                                    if (operand.Length > 1)
                                    {
                                        tempString = conTab.getOpCode(conTab.findOpCode(opcode)) + conTab.getRegisterValue(conTab.findRegister(operand.Substring(0, 1)))
                                            + conTab.getRegisterValue(conTab.findRegister(operand.Substring(operand.Length)));
                                        objCode.Add(tempString);
                                    }
                                    else
                                    {
                                        tempString = conTab.getOpCode(conTab.findOpCode(opcode)) + conTab.getRegisterValue(conTab.findRegister(operand.Substring(0, 1))) + "0";
                                        objCode.Add(tempString);
                                    }    
                                }
                                else if (addressMode.CompareTo("3") == 0)
                                {
                                    
                                }
                            }
                        }
                    }
                }
            } 
        }

        private String addModeThree(String opcode, int niBit, int xbpe, String address)
        {
        }

        private String addModeFour(String opcode, int niBit, int xbpe, String address)
        {
        }

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
        private int getBaseAddress(int location)
        {
        }
    }
    
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

    class SourceFile
    {
        List<FileFormat> buildSourceCode = new List<FileFormat>();
        String tempLine;
        String label;
        String sourceCode;
        String operand;
        String[] stringParse;
        int size = 0;

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
                                operand = null;
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
            }
        }

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
    }

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
            opcodeTable searchOpCodes = new opcodeTable(search, "", "");
            for (int i = 0; i < opTable.Count; i++)
            {
                if (opTable[i].CompareTo(searchOpCodes) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public String getOpCode(int i)
        {
            return opTable[i].getOpCode();
        }

        public String getMemAddressMode(int i)
        {
            return opTable[i].getType();
        }

        public int findRegister(String search)
        {
            registerTable searchRegisters = new registerTable(search, "");
            for (int j = 0; j < regTable.Count; j++)
            {
                if (regTable[j].CompareTo(searchRegisters) == 0)
                {
                    return j;
                }
            }
            return -1;
        }

        public String getRegisterValue(int j)
        {
            return regTable[j].getValue();
        }

    }
}
